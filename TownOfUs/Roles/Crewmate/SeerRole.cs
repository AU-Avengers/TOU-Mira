using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SeerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Seer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public static string ReworkString => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value ? "Alt" : string.Empty;
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}TabDescription");
    public List<string> ComparisonList = new ();

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            var sprite = TouCrewAssets.SeerSprite;
            var abilityName = TouLocale.GetParsed($"TouRole{LocaleKey}Reveal", "Reveal");
            var abilityDesc = TouLocale.GetParsed($"TouRole{LocaleKey}RevealWikiDescription");
            if (OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value)
            {
                abilityName = TouLocale.GetParsed($"TouRole{LocaleKey}Compare", "Compare");
                abilityDesc = TouLocale.GetParsed($"TouRole{LocaleKey}CompareWikiDescription");
                sprite = TouCrewAssets.SeerButtonSprites.AsEnumerable().Random()!;
            }
            return new List<CustomButtonWikiDescription>
            {
                new(abilityName, abilityDesc, sprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Seer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Seer,
        IntroSound = TouAudio.QuestionSound
    };
    public PlayerControl? GazeTarget { get; set; }
    public PlayerControl? IntuitTarget { get; set; }

    public static string TabHeaderString = TouLocale.GetParsed("TouRoleSeerTabHeader");
    public override void Initialize(PlayerControl player)
    {
        GazeTarget = null;
        IntuitTarget = null;
        RoleBehaviourStubs.Initialize(this, player);
        ComparisonList = new List<string>();
        TabHeaderString = TouLocale.GetParsed("TouRoleSeerTabHeader");
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Player.AmOwner)
        {
            var gazeButton = CustomButtonSingleton<SeerGazeButton>.Instance;
            gazeButton.ResetCooldownAndOrEffect();
            var intuitButton = CustomButtonSingleton<SeerIntuitButton>.Instance;
            intuitButton.ResetCooldownAndOrEffect();

            if (IntuitTarget != null)
            {
                ++intuitButton.UsesLeft;
                intuitButton.SetUses(intuitButton.UsesLeft);
                IntuitTarget = null;
            }

            if (GazeTarget != null)
            {
                ++gazeButton.UsesLeft;
                gazeButton.SetUses(gazeButton.UsesLeft);
                GazeTarget = null;
            }
        }
    }
    public void SeerCompare(PlayerControl seer)
    {
        if (GazeTarget == null || IntuitTarget == null)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>{TouLocale.GetParsed("TouRoleSeerCompareErrorAmountNotif")}</b>");
            return;
        }

        if (GazeTarget == seer || IntuitTarget == seer)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>{TouLocale.GetParsed("TouRoleSeerCompareErrorSelfNotif")}</b>");
            return;
        }
        var gazeButton = CustomButtonSingleton<SeerGazeButton>.Instance;
        gazeButton.ResetCooldownAndOrEffect();
        var intuitButton = CustomButtonSingleton<SeerIntuitButton>.Instance;
        intuitButton.ResetCooldownAndOrEffect();
        var playerA = GazeTarget.CachedPlayerData.PlayerName;
        var playerB = IntuitTarget.CachedPlayerData.PlayerName;

        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification(message, Color.white, new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Seer.LoadAsset());
            notif.AdjustNotification();
        }

        bool enemies = Enemies(GazeTarget, IntuitTarget);
        bool Enemies(PlayerControl p1, PlayerControl p2)
        {
            if (p1 == null || p2 == null) return false;
            if (p1.Data?.Role == null || p2.Data?.Role == null) return false;

            var friendlyNb = OptionGroupSingleton<SeerOptions>.Instance.BenignShowFriendlyToAll;
            var friendlyNe = OptionGroupSingleton<SeerOptions>.Instance.EvilShowFriendlyToAll;
            var friendlyNo = OptionGroupSingleton<SeerOptions>.Instance.OutlierShowFriendlyToAll;

            if (p1.IsCrewmate() && p2.IsCrewmate()) return false;
            if (p1.IsImpostor() && p2.IsImpostor()) return false;
            if (p1.Data.Role.Role == p2.Data.Role.Role) return false; // Two werewolves are friendly to one another
            if (p1.Is(RoleAlignment.NeutralBenign) && p2.Is(RoleAlignment.NeutralBenign)) return false;
            if (p1.Is(RoleAlignment.NeutralEvil) && p2.Is(RoleAlignment.NeutralEvil)) return false;
            if (p1.Is(RoleAlignment.NeutralOutlier) && p2.Is(RoleAlignment.NeutralOutlier)) return false;

            if (p1.Is(RoleAlignment.NeutralBenign) || p2.Is(RoleAlignment.NeutralBenign))
                return !friendlyNb;
            if (p1.Is(RoleAlignment.NeutralEvil) || p2.Is(RoleAlignment.NeutralEvil))
                return !friendlyNe;
            if (p1.Is(RoleAlignment.NeutralOutlier) || p2.Is(RoleAlignment.NeutralOutlier))
                return !friendlyNo;

            // You sense that Atony and Cursed Soul appear to be enemies!
            return true;
        }

        var players = new [] {playerA, playerB}.OrderBy(x => x.ToLowerInvariant()).ToArray();

        if (enemies)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft));
            var text = TouLocale.GetParsed("TouRoleSeerCompareEnemiesNotif").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ShowNotification($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{text}</color></b>");
            var compareResult = TouLocale.GetParsed("TouRoleSeerTabComparison").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ComparisonList.Add($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{compareResult.Replace("<num>", DeathEventHandlers.CurrentRound.ToString(TownOfUsPlugin.Culture))}</color></b>");
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.CrewmateBlue));
            var text = TouLocale.GetParsed("TouRoleSeerCompareFriendsNotif").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ShowNotification($"<b>{Palette.CrewmateBlue.ToTextColor()}{text}</color></b>");
            var compareResult = TouLocale.GetParsed("TouRoleSeerTabComparison").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ComparisonList.Add($"<b>{Palette.CrewmateBlue.ToTextColor()}{compareResult.Replace("<num>", DeathEventHandlers.CurrentRound.ToString(TownOfUsPlugin.Culture))}</color></b>");
        }
        IntuitTarget = null;
        GazeTarget = null;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (ComparisonList.Count != 0)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b>{TabHeaderString}</b>");
            foreach (var comparison in ComparisonList)
            {
                var newText = $"<b><size=70%>{comparison}</size></b>";
                stringB.AppendLine(TownOfUsPlugin.Culture, $"{newText}");
            }
        }

        return stringB;
    }
}