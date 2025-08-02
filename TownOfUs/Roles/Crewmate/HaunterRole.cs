using System.Collections;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

// using Reactor.Utilities.Extensions;

namespace TownOfUs.Roles.Crewmate;

public sealed class HaunterRole(IntPtr cppPtr) : CrewmateGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public bool Revealed { get; private set; }
    public bool CompletedAllTasks { get; private set; }

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }
    public bool CanBeClicked { get; set; }
    public bool GhostActive => Setup && !Caught;

    public bool CanCatch()
    {
        var options = OptionGroupSingleton<HaunterOptions>.Instance;

        if (options.HaunterCanBeClickedBy == HaunterRoleClickableType.ImpsOnly &&
            !PlayerControl.LocalPlayer.IsImpostor())
        {
            return false;
        }

        if (options.HaunterCanBeClickedBy == HaunterRoleClickableType.NonCrew &&
            !(PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling)
                || PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && allyMod.GetsPunished))
        {
            return false;
        }

        return true;
    }

    public void Spawn()
    {
        Setup = true;

        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Error($"Setup HaunterRole '{Player.Data.PlayerName}'");
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

    public void FadeUpdate(HudManager instance)
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

            // Logger<TownOfUsPlugin>.Message($"HaunterRole.FadeUpdate UnFaded");
        }
    }

    public void Clicked()
    {
        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Message($"HaunterRole.Clicked");
        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }

        Player.RemoveModifier<HaunterArrowModifier>();
    }

    public string RoleName => TouLocale.Get(TouNames.Haunter, "Haunter");
    public string RoleDescription => string.Empty;
    public string RoleLongDescription => "Complete all your tasks without getting caught to reveal impostors!";
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

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Ghost who can do tasks. They will appear as a transparent player. " +
            "If they finish all their tasks, all alive players will see who the Impostors are. " +
            "However, if an Impostor clicks them first, they will become a normal ghost. " +
            $"Impostors get a warning shortly before and when the {RoleName} finishes their tasks. "
            + MiscUtils.AppendOptionsText(GetType());
    }
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
        if (TutorialManager.InstanceExists)
        {
            Setup = true;

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

        var completedTasks = Player.myTasks.ToArray().Count(t => t.IsComplete);
        var tasksRemaining = Player.myTasks.Count - completedTasks;

        CanBeClicked = tasksRemaining <= (int)OptionGroupSingleton<HaunterOptions>.Instance.NumTasksLeftBeforeClickable;
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
        if (Caught)
        {
            return;
        }

        var completedTasks = Player.myTasks.ToArray().Count(t => t.IsComplete);
        var tasksRemaining = Player.myTasks.Count - completedTasks - 1;

        CanBeClicked = tasksRemaining <= (int)OptionGroupSingleton<HaunterOptions>.Instance.NumTasksLeftBeforeClickable;

        if (!CompletedAllTasks && completedTasks == Player.myTasks.Count)
        {
            CompletedAllTasks = true;

            if (Player.AmOwner || IsTargetOfHaunter(PlayerControl.LocalPlayer))
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.white));
            }
        }

        if (!Revealed && tasksRemaining == (int)OptionGroupSingleton<HaunterOptions>.Instance.NumTasksLeftBeforeAlerted)
        {
            // Logger<TownOfUsPlugin>.Error($"CheckTaskRequirements Revealed");
            Revealed = true;

            if (Player.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(RoleColor));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}You have alerted the Killers!</b></color>", Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }
            else if (IsTargetOfHaunter(PlayerControl.LocalPlayer))
            {
                // Logger<TownOfUsPlugin>.Error($"CheckTaskRequirements IsTargetOfHaunter");
                Coroutines.Start(MiscUtils.CoFlash(RoleColor));

                Player.AddModifier<HaunterArrowModifier>(PlayerControl.LocalPlayer, RoleColor);
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Haunter.ToTextColor()}A Haunter is loose, catch them before they reveal you!</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
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