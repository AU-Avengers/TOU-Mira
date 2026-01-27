using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Text;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SnitchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ILoyalCrewmate
{
    public bool CanBeTraitor => false;
    public bool CanBeCrewpostor => false;
    public bool CanBeEgotist => true;
    public bool CanBeOtherEvil => false;

    private Dictionary<byte, ArrowBehaviour>? _snitchArrows;
    [HideFromIl2Cpp] public ArrowBehaviour? SnitchRevealArrow { get; private set; }
    public bool CompletedAllTasks => TaskStage is TaskStage.CompletedTasks;
    public bool OnLastTask => TaskStage is TaskStage.Revealed or TaskStage.CompletedTasks;
    public TaskStage TaskStage { get; private set; } = TaskStage.Unrevealed;

    private void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not SnitchRole)
        {
            return;
        }

        if (SnitchRevealArrow != null && SnitchRevealArrow.target != SnitchRevealArrow.transform.parent.position)
        {
            SnitchRevealArrow.target = Player.transform.position;
        }

        if (_snitchArrows != null && _snitchArrows.Count > 0 && Player.AmOwner)
        {
            _snitchArrows.ToList().ForEach(arrow => arrow.Value.target = arrow.Value.transform.parent.position);
        }
    }

    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Snitch";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Snitch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Snitch,
        IntroSound = TouAudio.ToppatIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{RoleColor.ToTextColor()}{TouLocale.Get("YouAreA")}<b> {RoleName}.</b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(RoleAlignment, true)}</b></size>");
        stringB.Append("<size=70%>");

        var desc = CompletedAllTasks ? "CompletedTasks" : string.Empty;
        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            desc += "Ego";
        }

        var text = TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription{desc}");

        stringB.AppendLine(TownOfUsPlugin.Culture, $"{text}");

        return stringB;
    }

    public void CheckTaskRequirements()
    {
        UpdateTaskStage(silent: false, forceRecalculate: false);
    }

    public void RecalculateTaskStage()
    {
        UpdateTaskStage(silent: false, forceRecalculate: true);
    }

    public void RecalculateTaskStage(bool silent)
    {
        UpdateTaskStage(silent: silent, forceRecalculate: true);
    }

    private void UpdateTaskStage(bool silent, bool forceRecalculate)
    {
        if (Player == null)
        {
            return;
        }

        GetTaskCounts(Player, out var completedTasks, out var totalTasks);
        if (totalTasks <= 1)
        {
            if (forceRecalculate)
            {
                ClearArrows();
                TaskStage = TaskStage.Unrevealed;
            }
            return;
        }

        var tasksRemaining = totalTasks - completedTasks;
        var threshold = (int)OptionGroupSingleton<SnitchOptions>.Instance.TaskRemainingWhenRevealed;

        TaskStage newStage;
        if (completedTasks == totalTasks)
        {
            newStage = TaskStage.CompletedTasks;
        }
        else if (tasksRemaining <= threshold)
        {
            newStage = TaskStage.Revealed;
        }
        else
        {
            newStage = TaskStage.Unrevealed;
        }

        if (!forceRecalculate)
        {
            if (TaskStage is TaskStage.Unrevealed && newStage is TaskStage.Revealed || completedTasks == totalTasks && TaskStage is not TaskStage.CompletedTasks)
            {
                TaskStage = newStage;
                HandleStageChange(newStage, silent);
            }
            else
            {
                var textlog = $"Snitch Stage for '{Player.Data.PlayerName}': {TaskStage.ToDisplayString()} - ({completedTasks} / {totalTasks})";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlog);
                return;
            }
        }
        else
        {
            if (TaskStage != newStage)
            {
                ClearArrows();
            }

            TaskStage = newStage;
            HandleStageChange(newStage, silent);
        }
    }

    private void HandleStageChange(TaskStage stage, bool silent)
    {
        var textlog = $"Snitch Stage for '{Player.Data.PlayerName}': {stage.ToDisplayString()}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlog);

        if (stage is TaskStage.Revealed)
        {
            if (Player.AmOwner)
            {
                if (!silent)
                {
                    Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
                    var text = "The impostors know of your whereabouts!";
                    if (Player.HasModifier<EgotistModifier>())
                    {
                        text = "The impostors know of your whereabouts, and know you're the Egotist!";
                    }

                    var notif1 = Helpers.CreateAndShowNotification(
                        $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                        new Vector3(0f, 1f, -20f),
                        spr: TouRoleIcons.Snitch.LoadAsset());
                    notif1.AdjustNotification();
                }
            }
            else if (IsTargetOfSnitch(PlayerControl.LocalPlayer))
            {
                CreateRevealingArrow(silent);
                if (!silent)
                {
                    Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
                    var text = "The Snitch is getting closer to reveal you!";
                    if (Player.HasModifier<EgotistModifier>())
                    {
                        text = "The Snitch is an Egotist, who will help you overthrow the crewmates!";
                    }

                    var notif1 = Helpers.CreateAndShowNotification(
                        $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                        new Vector3(0f, 1f, -20f),
                        spr: TouRoleIcons.Snitch.LoadAsset());
                    notif1.AdjustNotification();
                }
            }
        }
        else if (stage is TaskStage.CompletedTasks)
        {
            if (Player.AmOwner)
            {
                CreateSnitchArrows(silent);
                if (!silent)
                {
                    var text = "You have revealed the impostors!";
                    if (Player.HasModifier<EgotistModifier>())
                    {
                        text = "You have revealed the impostors, who can help your win condition!";
                    }

                    var notif1 = Helpers.CreateAndShowNotification(
                        $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                        new Vector3(0f, 1f, -20f),
                        spr: TouRoleIcons.Snitch.LoadAsset());
                    notif1.AdjustNotification();
                }
            }
            else if (IsTargetOfSnitch(PlayerControl.LocalPlayer) && !silent)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
                var text = "The Snitch knows what you are now!";
                if (Player.HasModifier<EgotistModifier>())
                {
                    text = "The Snitch can now help you as the Egotist!";
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Snitch.LoadAsset());
                notif1.AdjustNotification();
            }
        }
    }

    private static void GetTaskCounts(PlayerControl player, out int completed, out int total)
    {
        completed = 0;
        total = 0;

        if (player == null || player.Data == null)
        {
            return;
        }

        if (player.myTasks != null && player.myTasks.Count > 0)
        {
            var tasks = player.myTasks.ToArray().Where(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());
            foreach (var t in tasks)
            {
                total++;
                var taskInfo = player.Data.FindTaskById(t.Id);
                var isComplete = taskInfo != null ? taskInfo.Complete : t.IsComplete;
                if (isComplete)
                {
                    completed++;
                }
            }

            return;
        }

        foreach (var info in player.Data.Tasks)
        {
            total++;
            if (info.Complete)
            {
                completed++;
            }
        }
    }

    public static bool IsTargetOfSnitch(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            return false;
        }

        return (player.IsImpostor() && !player.IsTraitor()) ||
               (player.IsTraitor() && OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor) ||
               (player.Is(RoleAlignment.NeutralKilling) &&
                OptionGroupSingleton<SnitchOptions>.Instance.SnitchNeutralRoles);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        ClearArrows();
        // incase amne becomes snitch or smth
        CheckTaskRequirements();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        ClearArrows();
    }

    public void RemoveArrowForPlayer(byte playerId)
    {
        if (_snitchArrows != null && _snitchArrows.TryGetValue(playerId, out var arrow))
        {
            arrow.gameObject.Destroy();
            _snitchArrows.Remove(playerId);
        }
    }

    public void ClearArrows()
    {
        if (_snitchArrows != null && _snitchArrows.Count > 0)
        {
            _snitchArrows.ToList().ForEach(arrow => arrow.Value.gameObject.Destroy());
            _snitchArrows.Clear();
        }
        // Set to null so CreateSnitchArrowsSilent() can recreate arrows when needed
        _snitchArrows = null;

        if (SnitchRevealArrow != null)
        {
            SnitchRevealArrow.gameObject.Destroy();
            SnitchRevealArrow = null;
        }

        // Remove modifiers from all players explicitly to ensure they're cleared on all clients
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null)
            {
                continue;
            }

            var modifiers = player.GetModifiers<SnitchImpostorRevealModifier>().ToList();
            foreach (var mod in modifiers)
            {
                player.GetModifierComponent()?.RemoveModifier(mod);
            }

            var playerMods = player.GetModifiers<SnitchPlayerRevealModifier>().ToList();
            foreach (var mod in playerMods)
            {
                player.GetModifierComponent()?.RemoveModifier(mod);
            }
        }
    }

    private void CreateRevealingArrow(bool silent = false)
    {
        if (SnitchRevealArrow != null)
        {
            return;
        }

        Player.AddModifier<SnitchPlayerRevealModifier>(
            RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SnitchRole>()));
        PlayerNameColor.Set(Player);
        if (!silent)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
        }
        SnitchRevealArrow = MiscUtils.CreateArrow(Player.transform, TownOfUsColors.Snitch);
    }

    private void CreateSnitchArrows(bool silent = false)
    {
        if (_snitchArrows != null)
        {
            return;
        }

        if (!silent)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
        }
        _snitchArrows = new Dictionary<byte, ArrowBehaviour>();
        var imps = Helpers.GetAlivePlayers().Where(plr => plr.Data.Role.IsImpostor && !plr.IsTraitor());
        var traitor = Helpers.GetAlivePlayers().FirstOrDefault(plr => plr.IsTraitor());
        imps.ToList().ForEach(imp =>
        {
            _snitchArrows.Add(imp.PlayerId, MiscUtils.CreateArrow(imp.transform, TownOfUsColors.Impostor));
            PlayerNameColor.Set(imp);
            imp.AddModifier<SnitchImpostorRevealModifier>();
        });

        if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && traitor != null)
        {
            _snitchArrows.Add(traitor.PlayerId, MiscUtils.CreateArrow(traitor.transform, TownOfUsColors.Impostor));
            PlayerNameColor.Set(traitor);
            traitor.AddModifier<SnitchImpostorRevealModifier>();
        }

        if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchNeutralRoles)
        {
            var neutrals = MiscUtils.GetRoles(RoleAlignment.NeutralKilling)
                .Where(role => !role.Player.Data.IsDead && !role.Player.Data.Disconnected);
            neutrals.ToList().ForEach(neutral =>
            {
                _snitchArrows.Add(neutral.Player.PlayerId,
                    MiscUtils.CreateArrow(neutral.Player.transform, TownOfUsColors.Neutral));
                PlayerNameColor.Set(neutral.Player);
                neutral.Player.AddModifier<SnitchImpostorRevealModifier>();
            });
        }
    }

    public void AddSnitchTraitorArrows()
    {
        if (PlayerControl.LocalPlayer.IsTraitor() && OnLastTask)
        {
            CreateRevealingArrow();
        }

        if (CompletedAllTasks && Player.AmOwner)
        {
            var traitor = Helpers.GetAlivePlayers().FirstOrDefault(plr => plr.IsTraitor());
            if (_snitchArrows == null || traitor == null ||
                (_snitchArrows.TryGetValue(traitor.PlayerId, out var arrow) && arrow != null))
            {
                return;
            }

            if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && traitor != null)
            {
                _snitchArrows.Add(traitor.PlayerId, MiscUtils.CreateArrow(traitor.transform, TownOfUsColors.Impostor));
                PlayerNameColor.Set(traitor);
                Player.AddModifier<SnitchImpostorRevealModifier>();
            }
        }
    }
}

public enum TaskStage
{
    Unrevealed,
    Revealed,
    CompletedTasks
}