using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class InquisitorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    IAssignableTargets, ICrewVariant, IContinuesGame, IUnlovable, IProgressTally
{
    public string GetHereticTally()
    {
        var killed = Targets.Count(x => x.Key.HasDied());
        var total = Targets.Count;
        var colorbase = Color.yellow;
        var color = Color.yellow;
        if (killed <= 0)
        {
            color = TownOfUsColors.ImpSoft;
        }
        else if (killed >= total)
        {
            color = TownOfUsColors.Doomsayer;
        }
        else if (killed > total / 2)
        {
            var fraction = ((killed * 0.4f) / total);
            Color color2 = TownOfUsColors.Doomsayer;
            color = new
            ((color2.r * fraction + colorbase.r * (1 - fraction)),
                (color2.g * fraction + colorbase.g * (1 - fraction)),
                (color2.b * fraction + colorbase.b * (1 - fraction)));
        }
        else if (killed < total / 2)
        {
            var fraction = ((killed * 0.9f) / total);
            Color color2 = TownOfUsColors.ImpSoft;
            color = new
            ((colorbase.r * fraction + color2.r * (1 - fraction)),
                (colorbase.g * fraction + color2.g * (1 - fraction)),
                (colorbase.b * fraction + color2.b * (1 - fraction)));
        }

        return $"{color.ToTextColor()}({killed}/{total})</color>";
    }
    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (amOwner || localDead)
        {
            progress = GetHereticTally();
            return true;
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => string.Empty;

    public string ProgressOnSummaryDetailed =>
        string.Empty;
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralOutlierTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public bool IsUnlovable => true;
    public bool ContinuesGame => !Player.HasDied() && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame && CanVanquish && !TargetsDead && Helpers.GetAlivePlayers().Count <= 3;
    public bool CanVanquish { get; set; } = true;

    [HideFromIl2Cpp] public Dictionary<PlayerControl, RoleBehaviour> Targets { get; set; } = [];

    public bool TargetsDead { get; set; }
    public int Priority { get; set; } = 5;

    public void AssignTargets()
    {
        if (!RoleOptions.IsClassicRoleAssignment)
        {
            return;
        }

        var inquis = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(x =>
                x.IsRole<InquisitorRole>() && !x.HasDied() &&
                !SpectatorRole.TrackedSpectators.Contains(x.Data.PlayerName));

        if (inquis == null)
        {
            var textlognotfound = $"Inquisitor not found.";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlognotfound);

            return;
        }

        var required = (int)OptionGroupSingleton<InquisitorOptions>.Instance.AmountOfHeretics;
        var players = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.Data.Role is not InquisitorRole && x.Data.Role is not SpectatorRole).ToList();
        var textlog = $"Players in heretic list possible: {players.Count}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, textlog);

        players.Shuffle();
        players.Shuffle();
        players.Shuffle();

        var evil = players.Any(x => x.IsNeutral() || x.IsImpostor())
            ? players.FirstOrDefault(x => x.IsNeutral() || x.IsImpostor())
            : players.Random();
        players.Remove(evil);
        players.Shuffle();

        var crew = players.Any(x => x.IsCrewmate()) ? players.FirstOrDefault(x => x.IsCrewmate()) : players.Random();
        players.Remove(crew);
        players.Shuffle();

        var random = players.Random();
        players.Remove(random);
        players.Shuffle();

        List<PlayerControl> filtered = [];

        if (evil != null)
        {
            filtered.Add(evil);
        }

        if (crew != null)
        {
            filtered.Add(crew);
        }

        if (random != null)
        {
            filtered.Add(random);
        }

        var other = players.Random();
        if (required is 4 or 5 && players.Count >= 1 && other != null)
        {
            filtered.Add(other);
            players.Remove(other);
        }

        players.Shuffle();
        other = players.Random();
        if (required is 5 && players.Count >= 1 && other != null)
        {
            filtered.Add(other);
        }

        if (filtered.Count > 0)
        {
            filtered = filtered.OrderBy(x => x.Data.Role.GetRoleName()).ToList();
            foreach (var player in filtered)
            {
                RpcAddInquisTarget(inquis, player);
            }
        }
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SheriffRole>());
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Inquisitor";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription").Replace("<symbol>", "<color=#D94291>$</color>") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Inquire", "Inquire"),
                    MiraLocaleManager.Get($"TouRole{IdPart}InquireWikiDescription"),
                    TouNeutAssets.InquireSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Vanquish", "Vanquish"),
                    MiraLocaleManager.Get($"TouRole{IdPart}VanquishWikiDescription"),
                    TouNeutAssets.InquisKillSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Inquisitor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Inquisitor.LoadAsset(), "TouMira.Role.Neutral.Inquisitor", 1.45f),
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Inquisitor,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool MetWinCon => TargetsDead;

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        // Needed for the 1v1. The end check can run mid-kill, before TargetsDead is set, so read live state instead - Divani :)
        if (Helpers.GetAlivePlayers().Contains(Player) && Helpers.GetAlivePlayers().Count <= 1 &&
            Targets.Count > 0)
        {
            return true;
        }

        if (!TargetsDead)
        {
            return false;
        }

        // Must stay below the solo win above; at the top of the method it blocked it.
        if (!OptionGroupSingleton<InquisitorOptions>.Instance.StallGame)
        {
            return false;
        }

        var result = Helpers.GetAlivePlayers().Contains(Player) && Helpers.GetAlivePlayers().Count <= 2 &&
                     MiscUtils.KillersAliveCount == 1;
        return result;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{MiraLocaleManager.Get("TouRoleInquisitorTabAddition")}</b>");
        foreach (var target in Targets)
        {
            var newText = target.Key.HasDied()
                ? $"<b><size=80%>{MiscUtils.GetRoleTmpIcon(target.Value)}<s>{target.Value.TeamColor.ToTextColor()}{target.Value.GetRoleName()}</color></s></size></b>"
                : $"<b><size=80%>{MiscUtils.GetRoleTmpIcon(target.Value)}{target.Value.TeamColor.ToTextColor()}{target.Value.GetRoleName()}</color></size></b>";
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{newText}");
        }

        return stringB;
    }

    public void OffsetButtons()
    {
        var canVent = LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.OffsetButtonsToggle.Value;
        var inquire = CustomButtonSingleton<InquisitorInquireButton>.Instance;
        var vanquish = CustomButtonSingleton<InquisitorVanquishButton>.Instance;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(inquire, !canVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(vanquish, !canVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
        }

        CanVanquish = true;

        // if Inuquisitor was revived
        if (Targets.Count == 0)
        {
            var newTargets = new Dictionary<PlayerControl, RoleBehaviour>();
            foreach (var heretic in ModifierUtils.GetActiveModifiers<InquisitorHereticModifier>()
                         .Where(x => x.Player != player).OrderBy([HideFromIl2Cpp](x) => x.TargetRole.GetRoleName()))
            {
                newTargets.Add(heretic.Player, heretic.TargetRole);
            }

            Targets = newTargets;
        }

        if (TutorialManager.InstanceExists && Targets.Count == 0 && Player.AmOwner && Player.IsHost() &&
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            Coroutines.Start(SetTutorialTargets(this));
        }
    }

    private static IEnumerator SetTutorialTargets(InquisitorRole inquis)
    {
        yield return new WaitForSeconds(0.01f);
        inquis.AssignTargets();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        if (TutorialManager.InstanceExists && Player.AmOwner)
        {
            var players = ModifierUtils.GetPlayersWithModifier<InquisitorHereticModifier>().ToList();
            players.Do(x => x.RpcRemoveModifier<InquisitorHereticModifier>());
        }

        if (!Player.HasModifier<BasicGhostModifier>() && TargetsDead)
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Player.AmOwner)
        {
            GenerateReport();
        }
    }

    private void GenerateReport()
    {
        Info($"Generating Inquisitor report");

        var reportBuilder = new StringBuilder();

        if (!Player)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        foreach (var player in GameData.Instance.AllPlayers.ToArray()
                     .Where(x => !x.Object.HasDied() && x.Object.HasModifier<InquisitorInquiredModifier>()))
        {
            var text = MiraLocaleManager.Get("TouRoleInquisitorInquiredNonHeretic")
                .Replace("<player>", player.PlayerName);
            if (player.Object.HasModifier<InquisitorHereticModifier>())
            {
                text = MiraLocaleManager.Get("TouRoleInquisitorInquiredHeretic").Replace("<player>", player.PlayerName);
                reportBuilder.AppendLine(TownOfUsPlugin.Culture,
                    $"{text}\n");
                var roles = Targets.Select(x => x.Value).ToList();
                var lastRole = roles[^1];

                if (roles.Count != 0)
                {
                    reportBuilder.Append(TownOfUsPlugin.Culture, $"(");
                    foreach (var role2 in roles)
                    {
                        if (role2 == lastRole)
                        {
                            reportBuilder.Append(TownOfUsPlugin.Culture,
                                $"{MiscUtils.GetHyperlinkText(lastRole)})");
                        }
                        else
                        {
                            reportBuilder.Append(TownOfUsPlugin.Culture,
                                $"{MiscUtils.GetHyperlinkText(role2)}, ");
                        }
                    }
                }
            }
            else
            {
                reportBuilder.AppendLine(TownOfUsPlugin.Culture,
                    $"{text}");
            }

            player.Object.RemoveModifier<InquisitorInquiredModifier>();
        }

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title =
                $"<color=#{TownOfUsColors.Inquisitor.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TouRoleInquisitorMessageTitle")}</color>";
            MiscUtils.AddFakeChat(Player.Data, title, report, false, true);
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return TargetsDead || WinConditionMet();
    }

    public void CheckTargetDeath(PlayerControl exiled)
    {
        if (Player.HasDied())
        {
            return;
        }

        if (Targets.Count == 0)
        {
            return;
        }

        if (Targets.All(x => x.Key.HasDied() || x.Key == exiled))
            // Error($"CheckTargetEjection - exiled: {exiled.Data.PlayerName}");
        {
            InquisitorWin(Player);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AddInquisTarget)]
    public static void RpcAddInquisTarget(PlayerControl player, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not InquisitorRole)
        {
            Error("RpcAddInquisTarget - Invalid Inquisitor");
            return;
        }

        if (target == null)
        {
            return;
        }

        var role = player.GetRole<InquisitorRole>();

        if (role == null)
        {
            return;
        }

        role.Targets.Add(target, target.Data.Role);
        target.AddModifier<InquisitorHereticModifier>();
    }

    [MethodRpc((uint)TownOfUsRpc.InquisitorWin)]
    public static void RpcInquisitorWin(PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        InquisitorWin(player);
    }

    public static void InquisitorWin(PlayerControl player)
    {
        if (player.Data.Role is not InquisitorRole)
        {
            Error("RpcInquisitorWin - Invalid Inquisitor");
            return;
        }

        var exe = player.GetRole<InquisitorRole>();
        exe!.TargetsDead = true;
    }
}