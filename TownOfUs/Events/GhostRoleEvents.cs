using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events;

public static class GhostRoleEvents
{
    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        var player = @event.Player;
        if (player && player.AmOwner && HudManager.InstanceExists)
        {
            HudManager.Instance.SetHudActive(false);

            if (!MeetingHud.Instance)
            {
                HudManager.Instance.SetHudActive(true);
                Coroutines.Start(CoRemoveChat());
            }
        }
    }

    public static IEnumerator CoRemoveChat()
    {
        var toHide = OptionGroupSingleton<PostmortemOptions>.Instance.HideChatButton.Value &&
                     OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() <
                     RoleDistribution.HideAndSeek;
        
        if (toHide)
        {
            HudManager.Instance.Chat.SetVisible(false);
            HudManager.Instance.Chat.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.1f);
        if (toHide)
        {
            HudManager.Instance.Chat.SetVisible(false);
            HudManager.Instance.Chat.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.1f);
        
        if (toHide)
        {
            HudManager.Instance.Chat.SetVisible(false);
            HudManager.Instance.Chat.gameObject.SetActive(false);
        }
    }
    public static bool IsConsoleAllowed(this Console? console)
    {
        if (OptionGroupSingleton<GameMechanicOptions>.Instance.GhostwalkerFixSabos.Value || console == null)
        {
            return true;
        }

        if (console.TaskTypes.Contains(TaskTypes.ResetReactor) ||
            console.TaskTypes.Contains(TaskTypes.ResetSeismic) ||
            console.TaskTypes.Contains(TaskTypes.RestoreOxy) ||
            console.TaskTypes.Contains(TaskTypes.FixLights) ||
            console.TaskTypes.Contains(TaskTypes.FixComms))
        {
            return false;
        }

        return true;
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (!PlayerControl.LocalPlayer || !@event.NewRole)
        {
            return;
        }

        var player = @event.Player;
        if (@event.NewRole.Role is RoleTypes.GuardianAngel or RoleTypes.SpiritGuide && !player.HasModifier<BasicGhostModifier>())
        {
            player.AddModifier<BasicGhostModifier>();
        }
    }
    
    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
            !PlayerControl.LocalPlayer.Data.Role || PlayerControl.LocalPlayer.Data.Role is not IGhostRole ghostwalker)
        {
            return;
        }

        var console = @event.Usable.TryCast<Console>();
        if (!console || console.IsConsoleAllowed() || !ghostwalker.GhostActive)
        {
            return;
        }

        @event.Cancel();
    }

    [RegisterEvent(10000)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        Coroutines.Start(CoSetGhostwalkers(exiled));
    }

    public static IEnumerator CoSetGhostwalkers(PlayerControl? exiled)
    {
        yield return new WaitForSeconds(1f);

        var basicGhosts = PlayerControl.AllPlayerControls.ToArray().ToList();
        
        foreach (var role in MiscUtils.AllTouRoles.OfType<IBasicGhostRole>().OrderBy(x => x.SpawnPriority))
        {
            basicGhosts = role.GetAvailableGhosts(basicGhosts, exiled);
        }
    }

    [RegisterEvent(10000000)]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        foreach (var ghost in CustomRoleUtils.GetActiveRoles().OfType<IGhostRole>())
        {
            if (ghost.Caught)
            {
                continue;
            }

            ghost.Spawn();
        }
    }
}