using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
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
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events;

public static class GhostRoleEvents
{
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
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var player = @event.Player;
        if (@event.NewRole is GuardianAngelRole && !player.HasModifier<BasicGhostModifier>())
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

        var haunterData = MiscUtils.GetAssignData((RoleTypes)RoleId.Get<HaunterRole>());

        if (CustomRoleUtils.GetActiveRoles().OfType<HaunterRole>().Count() < haunterData.Count)
        {
            var isSkipped = haunterData.Chance < 100 && HashRandom.Next(101) > haunterData.Chance;

            if (!isSkipped)
            {
                var deadCrew = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                    (x.Data.IsDead || x == exiled) && x.GetRoleWhenAlive().IsCrewmate() && !x.HasModifier<AllianceGameModifier>() &&
                    x.CanGetGhostRole() &&
                    x.Data.Role).ToList();

                if (deadCrew.Count > 0)
                {
                    deadCrew.Shuffle();

                    var player = deadCrew.TakeFirst();

                    if (player != null)
                    {
                        player.RpcChangeRole(RoleId.Get<HaunterRole>());
                    }
                }
            }
        }

        var phantomData = MiscUtils.GetAssignData((RoleTypes)RoleId.Get<SpectreRole>());

        if (CustomRoleUtils.GetActiveRoles().OfType<SpectreRole>().Count() < phantomData.Count)
        {
            var isSkipped = phantomData.Chance < 100 && HashRandom.Next(101) > phantomData.Chance;

            if (!isSkipped)
            {
                var deadNeutral = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                    (x.Data.IsDead || x == exiled) && x.GetRoleWhenAlive().IsNeutral() &&
                    !x.GetRoleWhenAlive().DidWin(GameOverReason.CrewmatesByVote) &&
                    x.CanGetGhostRole() &&
                    !x.HasModifier<AllianceGameModifier>()).ToList();

                if (deadNeutral.Count > 0)
                {
                    deadNeutral.Shuffle();

                    var player = deadNeutral.TakeFirst();

                    if (player != null)
                    {
                        player.RpcChangeRole(RoleId.Get<SpectreRole>());
                    }
                }
            }
        }
    }

    [RegisterEvent]
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