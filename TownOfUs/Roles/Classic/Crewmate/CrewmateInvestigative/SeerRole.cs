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
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SeerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Seer";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public static string ReworkString => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value ? "Alt" : string.Empty;
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}{ReworkString}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}{ReworkString}TabDescription");
    public List<string> ComparisonList = [];

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}{ReworkString}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            var sprite = TouCrewAssets.SeerSprite;
            var abilityName = MiraLocaleManager.Get($"TouRole{IdPart}Reveal", "Reveal");
            var abilityDesc = MiraLocaleManager.Get($"TouRole{IdPart}RevealWikiDescription");
            if (OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value)
            {
                abilityName = MiraLocaleManager.Get($"TouRole{IdPart}Compare", "Compare");
                abilityDesc = MiraLocaleManager.Get($"TouRole{IdPart}CompareWikiDescription");
                sprite = TouCrewAssets.SeerButtonSprites.AsEnumerable().Random()!;
            }
            return
            [
                new(abilityName, abilityDesc, sprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Seer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Seer.LoadAsset(), "TouMira.Role.Crewmate.Seer", 1.45f),
        Icon = TouRoleIcons.Seer,
        OptionsScreenshot = TouBanners.SeerRoleBanner,
        IntroSound = TouAudio.QuestionSound
    };
    [HideFromIl2Cpp] public PlayerControl? GazeTarget { get; set; }
    [HideFromIl2Cpp] public PlayerControl? IntuitTarget { get; set; }
    [HideFromIl2Cpp] public List<PlayerControl> ComparedPlayers { get; } = [];
    public bool UsedThisRound { get; set; }

    public static string TabHeaderString = MiraLocaleManager.Get("TouRoleSeerTabHeader");
    public override void Initialize(PlayerControl player)
    {
        GazeTarget = null;
        IntuitTarget = null;
        RoleBehaviourStubs.Initialize(this, player);
        ComparisonList = [];
        ComparedPlayers.Clear();
        TabHeaderString = MiraLocaleManager.Get("TouRoleSeerTabHeader");
    }

    [HideFromIl2Cpp]
    public bool CanCompare(PlayerControl player)
    {
        return !OptionGroupSingleton<SeerOptions>.Instance.CompareEachPlayerOnce || !ComparedPlayers.Contains(player);
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
            ShowNotification($"<b>{MiraLocaleManager.Get("TouRoleSeerCompareErrorAmountNotif")}</b>");
            return;
        }

        if (GazeTarget == seer || IntuitTarget == seer)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>{MiraLocaleManager.Get("TouRoleSeerCompareErrorSelfNotif")}</b>");
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
            var text = MiraLocaleManager.Get("TouRoleSeerCompareEnemiesNotif").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ShowNotification($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{text}</color></b>");
            var compareResult = MiraLocaleManager.Get("TouRoleSeerTabComparison").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ComparisonList.Add($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{compareResult.Replace("<num>", HudManagerHelper.Instance.CurrentRound.ToString(TownOfUsPlugin.Culture))}</color></b>");
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.CrewmateBlue));
            var text = MiraLocaleManager.Get("TouRoleSeerCompareFriendsNotif").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ShowNotification($"<b>{Palette.CrewmateBlue.ToTextColor()}{text}</color></b>");
            var compareResult = MiraLocaleManager.Get("TouRoleSeerTabComparison").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ComparisonList.Add($"<b>{Palette.CrewmateBlue.ToTextColor()}{compareResult.Replace("<num>", HudManagerHelper.Instance.CurrentRound.ToString(TownOfUsPlugin.Culture))}</color></b>");
        }

        if (GazeTarget != null)
        {
            ComparedPlayers.Add(GazeTarget);
        }

        if (IntuitTarget != null)
        {
            ComparedPlayers.Add(IntuitTarget);
        }

        IntuitTarget = null;
        GazeTarget = null;
        UsedThisRound = true;
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