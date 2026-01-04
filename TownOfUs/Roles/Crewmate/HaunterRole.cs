using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

// using Reactor.Utilities.Extensions;

namespace TownOfUs.Roles.Crewmate;

public sealed class HaunterRole(IntPtr cppPtr) : CrewmateGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public bool Revealed => TaskStage is GhostTaskStage.Revealed or GhostTaskStage.CompletedTasks;
    public bool CompletedAllTasks => TaskStage is GhostTaskStage.CompletedTasks;

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }

    public bool CanBeClicked
    {
        get
        {
            return TaskStage is GhostTaskStage.Clickable || TaskStage is GhostTaskStage.Revealed ||
                   TaskStage is GhostTaskStage.CompletedTasks;
        }
        set
        {
            // Left Alone
        }
    }

    public GhostTaskStage TaskStage { get; private set; } = GhostTaskStage.Unclickable;
    public bool GhostActive => Setup && !Caught;

    public bool CanCatch()
    {
        var options = OptionGroupSingleton<HaunterOptions>.Instance;

        if (options.HaunterCanBeClickedBy == HaunterRoleClickableType.ImpsOnly &&
            !PlayerControl.LocalPlayer.IsImpostorAligned())
        {
            return false;
        }

        if (options.HaunterCanBeClickedBy == HaunterRoleClickableType.NonCrew &&
            !(PlayerControl.LocalPlayer.IsImpostorAligned() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling)
                                                     || PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(
                                                         out var allyMod) && allyMod.GetsPunished))
        {
            return false;
        }

        return true;
    }

    public void Spawn()
    {
        Setup = true;

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.SetCamouflage(false);
        }

        var text = $"Setup HaunterRole '{Player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

        Player.gameObject.layer = LayerMask.NameToLayer("Players");

        Player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        Player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => Player.OnClick()));
        Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;

        if (Player.AmOwner)
        {
            Player.SpawnAtRandomVent();
            Player.MyPhysics.ResetMoveState();

            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
            HudManager.Instance.AbilityButton.SetDisabled();
            HudManagerPatches.ResetZoom();
        }
    }

    public void FadeUpdate()
    {
        if (!Caught && Setup)
        {
            Player.GhostFade();
            Faded = true;
        }
        else if (Faded)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;

            // Message($"HaunterRole.FadeUpdate UnFaded");
        }
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not HaunterRole || MeetingHud.Instance)
        {
            return;
        }

        FadeUpdate();
    }

    public void Clicked()
    {
        var text = $"Clicked HaunterRole: '{Player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Message, text);

        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }

        Player.RemoveModifier<HaunterArrowModifier>();
    }

    public string LocaleKey => "Haunter";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Haunter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Haunter,
        TasksCountForProgress = false,
        HideSettings = false,
        ShowInFreeplay = true
    };


    // public DangerMeter ImpostorMeter { get; set; }

    public override void UseAbility()
    {
        if (GhostActive)
        {
            return;
        }

        base.UseAbility();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
        if (TutorialManager.InstanceExists)
        {
            Setup = true;

            if (HudManagerPatches.CamouflageCommsEnabled)
            {
                Player.SetCamouflage(false);
            }

            Coroutines.Start(SetTutorialCollider(Player));

            if (Player.AmOwner)
            {
                Player.MyPhysics.ResetMoveState();

                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.SetHudActive(true);
                HudManager.Instance.AbilityButton.SetDisabled();
                HudManagerPatches.ResetZoom();
                // var dangerMeter = GameManagerCreator.Instance.HideAndSeekManagerPrefab.LogicDangerLevel.dangerMeter;
                // ImpostorMeter = UnityEngine.Object.Instantiate(dangerMeter, HudManager.Instance.transform.parent);
            }
        }

        MiscUtils.AdjustGhostTasks(player);
    }

    /* public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not HaunterRole || !Player.AmOwner) return;

        float num = float.MaxValue;
        var dangerLevel1 = 0f;
        var dangerLevel2 = 0f;
        var scaryMusicDistance = 55f * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod;
        var veryScaryMusicDistance = 15f * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod;
        foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsImpostor() || x.IsNeutral()))
        {
            if (player != null)
            {
                float sqrMagnitude = (player.transform.position - Player.transform.position).sqrMagnitude;
                if (sqrMagnitude < scaryMusicDistance && num > sqrMagnitude)
                {
                    num = sqrMagnitude;
                }
            }
        }
        dangerLevel1 = Mathf.Clamp01((scaryMusicDistance - num) / (scaryMusicDistance - veryScaryMusicDistance));
        dangerLevel2 = Mathf.Clamp01((veryScaryMusicDistance - num) / veryScaryMusicDistance);
        ImpostorMeter.SetDangerValue(dangerLevel1, dangerLevel2);
    } */
    private static IEnumerator SetTutorialCollider(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        player.gameObject.layer = LayerMask.NameToLayer("Players");

        player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => player.OnClick()));
        player.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (TutorialManager.InstanceExists)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;
        }
        /* if (Player.AmOwner)
        {
            ImpostorMeter.DestroyImmediate();
        } */
    }


    public override bool CanUse(IUsable console)
    {
        var validUsable = console.TryCast<Console>() ||
                          console.TryCast<DoorConsole>() ||
                          console.TryCast<OpenDoorConsole>() ||
                          console.TryCast<DeconControl>() ||
                          console.TryCast<PlatformConsole>() ||
                          console.TryCast<Ladder>() ||
                          console.TryCast<ZiplineConsole>();

        return GhostActive && validUsable;
    }

    public void CheckTaskRequirements()
    {
        UpdateTaskStage(silent: false, forceRecalculate: false);
    }

    private void UpdateTaskStage(bool silent, bool forceRecalculate)
    {
        if (Caught || Player == null)
        {
            return;
        }

        GetTaskCounts(Player, out var completedTasks, out var totalTasks);
        var tasksRemaining = totalTasks - completedTasks;

        var opt = OptionGroupSingleton<HaunterOptions>.Instance;
        var clickableAt = (int)opt.NumTasksLeftBeforeClickable;
        var alertedAt = (int)opt.NumTasksLeftBeforeAlerted;

        GhostTaskStage newStage;
        if (totalTasks > 0 && completedTasks == totalTasks)
        {
            newStage = GhostTaskStage.CompletedTasks;
        }
        else if (tasksRemaining <= alertedAt)
        {
            newStage = GhostTaskStage.Revealed;
        }
        else if (tasksRemaining <= clickableAt)
        {
            newStage = GhostTaskStage.Clickable;
        }
        else
        {
            newStage = GhostTaskStage.Unclickable;
        }

        if (!forceRecalculate)
        {
            if (TaskStage is GhostTaskStage.Unclickable && newStage is GhostTaskStage.Clickable || !Revealed && newStage is GhostTaskStage.Revealed || !CompletedAllTasks && newStage is GhostTaskStage.CompletedTasks)
            {
                TaskStage = newStage;
                HandleStageChange(newStage, silent);
            }
            else
            {
                var text = $"Haunter Stage for '{Player.Data.PlayerName}': {TaskStage.ToDisplayString()} - ({completedTasks} / {totalTasks})";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);
            }
        }
        else
        {
            if (TaskStage != newStage)
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p == null)
                    {
                        continue;
                    }

                    var modifiers = p.GetModifiers<HaunterArrowModifier>().ToList();
                    foreach (var mod in modifiers)
                    {
                        if (mod.Owner == Player)
                        {
                            p.GetModifierComponent()?.RemoveModifier(mod);
                        }
                    }
                }
            }

            TaskStage = newStage;
            HandleStageChange(newStage, silent);
        }
    }

    private void HandleStageChange(GhostTaskStage stage, bool silent)
    {
        var text = $"Haunter Stage for '{Player.Data.PlayerName}': {stage.ToDisplayString()}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

        if (stage is GhostTaskStage.Clickable)
        {
            if (Player.AmOwner && !silent)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}You are now clickable by players!</b></color>",
                    Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.AdjustNotification();
            }
        }
        else if (stage is GhostTaskStage.Revealed)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.AmOwner)
                {
                    continue;
                }

                if (IsTargetOfHaunter(player))
                {
                    Player.AddModifier<HaunterArrowModifier>(player, RoleColor);
                }
            }

            if (Player.AmOwner && !silent)
            {
                Coroutines.Start(MiscUtils.CoFlash(RoleColor));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}You have alerted the Killers!</b></color>", Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (IsTargetOfHaunter(PlayerControl.LocalPlayer) && !silent)
            {
                Coroutines.Start(MiscUtils.CoFlash(RoleColor));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}A Haunter is loose, catch them before they reveal you!</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.AdjustNotification();
            }
        }
        else if (stage is GhostTaskStage.CompletedTasks)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.AmOwner)
                {
                    continue;
                }

                if (IsTargetOfHaunter(player))
                {
                    Player.AddModifier<HaunterArrowModifier>(player, RoleColor);
                }
            }

            if (Player.AmOwner && !silent)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.white));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}You have revealed the Killers!</b></color>", Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (IsTargetOfHaunter(PlayerControl.LocalPlayer) && !silent)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.white));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}The Haunter has completed their tasks!</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
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

    public static bool IsTargetOfHaunter(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            return false;
        }

        return player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) &&
                                       OptionGroupSingleton<HaunterOptions>.Instance.RevealNeutralRoles);
    }

    public static bool HaunterVisibilityFlag(PlayerControl player)
    {
        var haunter = MiscUtils.GetRole<HaunterRole>();

        if (haunter == null)
        {
            return false;
        }

        return IsTargetOfHaunter(player) && haunter.CompletedAllTasks && !player.AmOwner;
    }
}