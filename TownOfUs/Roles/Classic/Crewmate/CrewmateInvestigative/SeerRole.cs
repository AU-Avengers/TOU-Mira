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
    public static string ReworkString => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value ? "Alt" : string.Empty;
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}{ReworkString}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}{ReworkString}.TabDescription");
    public List<string> ComparisonList = [];

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}{ReworkString}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            var sprite = TouCrewAssets.SeerSprite;
            var abilityName = MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Reveal", "Reveal");
            var abilityDesc = MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Reveal.WikiDescription");
            if (OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value)
            {
                abilityName = MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Compare", "Compare");
                abilityDesc = MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Compare.WikiDescription");
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
    [HideFromIl2Cpp] public List<PendingComparison> PendingComparisons { get; } = [];
    public bool UsedThisRound { get; set; }

    public static string TabHeaderString = MiraLocaleManager.Get("TownOfUsMira.Role.SeerTabHeader");
    public override void Initialize(PlayerControl player)
    {
        GazeTarget = null;
        IntuitTarget = null;
        RoleBehaviourStubs.Initialize(this, player);
        ComparisonList = [];
        ComparedPlayers.Clear();
        PendingComparisons.Clear();
        TabHeaderString = MiraLocaleManager.Get("TownOfUsMira.Role.SeerTabHeader");
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

            RevealPendingComparisons();
        }
    }

    private void RevealPendingComparisons()
    {
        if (Player.HasDied() || PendingComparisons.Count == 0)
        {
            return;
        }

        var title = $"<color=#{TownOfUsColors.Seer.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.SeerMessageTitle")}</color>";

        foreach (var pending in PendingComparisons.ToArray())
        {
            if (!pending.First.HasDied() && !pending.Second.HasDied())
            {
                continue;
            }

            ComparisonList[pending.Index] = pending.Entry;
            PendingComparisons.Remove(pending);
            MiscUtils.AddFakeChat(Player.Data, title, pending.Message, false, true);
        }
    }
    public void SeerCompare(PlayerControl seer)
    {
        if (GazeTarget == null || IntuitTarget == null)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.SeerCompareErrorAmountNotif")}</b>");
            return;
        }

        if (GazeTarget == seer || IntuitTarget == seer)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.SeerCompareErrorSelfNotif")}</b>");
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

        var resultColor = enemies ? TownOfUsColors.ImpSoft : Palette.CrewmateBlue;
        var resultKey = enemies ? "TownOfUsMira.Role.SeerCompareEnemiesNotif" : "TownOfUsMira.Role.SeerCompareFriendsNotif";

        var text = MiraLocaleManager.Get(resultKey).Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
        var compareResult = MiraLocaleManager.Get("TownOfUsMira.Role.SeerTabComparison").Replace("<gazed>", players[0]).Replace("<intuited>", players[1])
            .Replace("<num>", HudManagerHelper.Instance.CurrentRound.ToString(TownOfUsPlugin.Culture));
        var resultEntry = $"<b>{resultColor.ToTextColor()}{compareResult}</color></b>";

        if (OptionGroupSingleton<SeerOptions>.Instance.DelayedCompare.Value)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Seer));
            var delayed = MiraLocaleManager.Get("TownOfUsMira.Role.SeerCompareDelayedNotif").Replace("<gazed>", players[0]).Replace("<intuited>", players[1]);
            ShowNotification($"<b>{TownOfUsColors.Seer.ToTextColor()}{delayed}</color></b>");
            ComparisonList.Add($"<color=#BFBFBF>{compareResult}</color>");
            PendingComparisons.Add(new PendingComparison(GazeTarget, IntuitTarget, resultEntry, text, ComparisonList.Count - 1));
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(resultColor));
            ShowNotification($"<b>{resultColor.ToTextColor()}{text}</color></b>");
            ComparisonList.Add(resultEntry);
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
        var options = OptionGroupSingleton<SeerOptions>.Instance;

        if (options.SalemSeer.Value && options.DelayedCompare.Value)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{MiraLocaleManager.Get("TownOfUsMira.Role.SeerDelayedCompareAvailable")}</color></size>");
        }

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

public sealed record PendingComparison(PlayerControl First, PlayerControl Second, string Entry, string Message, int Index);