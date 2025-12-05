using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events;

public static class GhostRoleEvents
{
    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
            !PlayerControl.LocalPlayer.Data.Role)
        {
            return;
        }

        if (!@event.Usable.TryCast<Console>() ||
            PlayerControl.LocalPlayer.Data.Role is not IGhostRole { GhostActive: false })
        {
            return;
        }

        @event.Cancel();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            var haunterData = MiscUtils.GetAssignData((RoleTypes)RoleId.Get<HaunterRole>());

            if (CustomRoleUtils.GetActiveRoles().OfType<HaunterRole>().Count() < haunterData.Count)
            {
                var isSkipped = haunterData.Chance < 100 && HashRandom.Next(101) > haunterData.Chance;

                if (!isSkipped)
                {
                    var deadCrew = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                        x.Data.IsDead && x.GetRoleWhenAlive().IsCrewmate() && !x.HasModifier<AllianceGameModifier>() &&
                        !x.CanGetGhostRole() &&
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
                        x.Data.IsDead && x.GetRoleWhenAlive().IsNeutral() && !x.Data.Role.DidWin(GameOverReason.CrewmatesByVote) &&
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

        Coroutines.Start(SpawnCoroutine());
    }

    private static IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
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