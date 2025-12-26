using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Roles.Neutral;

public sealed class SpectreRole(IntPtr cppPtr)
    : NeutralGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public bool CompletedAllTasks => TaskStage is GhostTaskStage.CompletedTasks;

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }

    public bool CanBeClicked
    {
        get { return TaskStage is GhostTaskStage.Clickable or GhostTaskStage.Revealed; }
        set
        {
            // Left Alone
        }
    }

    public GhostTaskStage TaskStage { get; private set; } = GhostTaskStage.Unclickable;
    public bool GhostActive => Setup && !Caught;

    public bool CanCatch()
    {
        return true;
    }

    public void Spawn()
    {
        Setup = true;

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.SetCamouflage(false);
        }

        var textlog = $"Setup SpectreRole: '{Player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlog);

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
        }
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not SpectreRole || MeetingHud.Instance)
        {
            return;
        }

        FadeUpdate();
    }

    public void Clicked()
    {
        var textlog = $"Clicked SpectreRole: '{Player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Message, textlog);

        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }
    }

    public string LocaleKey => "Spectre";
    public override string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public override string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public override string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public override Color RoleColor => TownOfUsColors.Phantom;
    public override RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public override CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spectre,
        HideSettings = false,
        ShowInFreeplay = true
    };

    public bool MetWinCon => CompletedAllTasks;

    public override bool WinConditionMet()
    {
        return OptionGroupSingleton<SpectreOptions>.Instance.SpectreWin is SpectreWinOptions.EndsGame &&
               CompletedAllTasks;
    }



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
            }
        }

        MiscUtils.AdjustGhostTasks(player);
    }

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

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return CompletedAllTasks;
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

        var clickableAt = (int)OptionGroupSingleton<SpectreOptions>.Instance.NumTasksLeftBeforeClickable;
        GhostTaskStage newStage;
        if (totalTasks > 0 && completedTasks == totalTasks)
        {
            newStage = GhostTaskStage.CompletedTasks;
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
            if (TaskStage is GhostTaskStage.Unclickable && newStage is GhostTaskStage.Clickable || totalTasks > 0 && completedTasks == totalTasks && TaskStage is not GhostTaskStage.CompletedTasks)
            {
                TaskStage = newStage;
                HandleStageChange(newStage, silent);
            }
            else
            {
                var textlog = $"Spectre Stage for '{Player.Data.PlayerName}': {TaskStage.ToDisplayString()} - ({completedTasks} / {totalTasks})";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlog);
            }
        }
        else
        {
            if (newStage is not GhostTaskStage.CompletedTasks)
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p == null)
                    {
                        continue;
                    }

                    var modifiers = p.GetModifiers<MisfortuneTargetModifier>().ToList();
                    foreach (var mod in modifiers)
                    {
                        p.GetModifierComponent()?.RemoveModifier(mod);
                    }
                }

                var spookButton = CustomButtonSingleton<PhantomSpookButton>.Instance;
                spookButton.Show = false;
            }

            TaskStage = newStage;
            HandleStageChange(newStage, silent);
        }
    }

    private void HandleStageChange(GhostTaskStage stage, bool silent)
    {
        var textlog = $"Spectre Stage for '{Player.Data.PlayerName}': {stage.ToDisplayString()}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlog);

        if (stage is GhostTaskStage.Clickable)
        {
            if (Player.AmOwner && !silent)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Phantom.ToTextColor()}You are now clickable by players!</b></color>",
                    Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spectre.LoadAsset());
                notif1.AdjustNotification();
            }
        }
        else if (stage is GhostTaskStage.CompletedTasks)
        {
            if (OptionGroupSingleton<SpectreOptions>.Instance.SpectreWin is SpectreWinOptions.Spooks)
            {
                var allVictims = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => !x.AmOwner);

                if (allVictims.Any())
                {
                    foreach (var player in allVictims)
                    {
                        player.AddModifier<MisfortuneTargetModifier>();
                    }

                    if (Player.AmOwner)
                    {
                        var spookButton = CustomButtonSingleton<PhantomSpookButton>.Instance;
                        spookButton.Show = true;
                        spookButton.SetActive(true, this);
                    }
                }
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
}

public enum GhostTaskStage
{
    Unclickable,
    Clickable,
    Revealed,
    CompletedTasks
}