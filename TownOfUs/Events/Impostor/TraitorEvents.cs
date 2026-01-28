using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Impostor;

public static class TraitorEvents
{
    [RegisterEvent(-1)]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        var canSendLocally = source.IsHost() ? victim.AmOwner : source.AmOwner;

        if (source == victim || !canSendLocally || !source.HasModifier<CrewpostorModifier>() || !source.IsCrewmate() || Helpers.GetAlivePlayers().Any(x => x.IsImpostor()))
        {
            return;
        }
        // If no impostors remain, then the crewpostor will become an impostor to fix the end game result
        ToBecomeTraitorModifier.RpcSetTraitor(source);
    }
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        var crewpostor = ModifierUtils.GetActiveModifiers<CrewpostorModifier>()
            .FirstOrDefault(x => x.Player.IsCrewmate());
        if (@event.TriggeredByIntro)
        {
            if (crewpostor != null && crewpostor.Player.AmOwner)
            {
                var traitorRole = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TraitorRole>());
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouModifierCrewpostorIntroMessage")
                        .Replace("<modifier>",
                            $"{TownOfUsColors.Impostor.ToTextColor()}{crewpostor.ModifierName}</color>")
                        .Replace("<role>",
                            $"{TownOfUsColors.Impostor.ToTextColor()}{traitorRole.GetRoleName()}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Crewpostor.LoadAsset());

                notif1.AdjustNotification();
            }
            return;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }
        var alives = Helpers.GetAlivePlayers().ToList();
        if (crewpostor != null)
        {
            if (crewpostor.Player.HasDied() || alives.Any(x => x.IsImpostor()))
            {
                return;
            }
            ToBecomeTraitorModifier.RpcSetTraitor(crewpostor.Player);
            return;
        }
        var traitor = ModifierUtils.GetActiveModifiers<ToBecomeTraitorModifier>()
            .Where(x => !x.Player.HasDied() && x.Player.IsCrewmate()).Random();
        if (traitor != null)
        {
            if (alives.Count < OptionGroupSingleton<TraitorOptions>.Instance.LatestSpawn)
            {
                return;
            }

            foreach (var player in alives)
            {
                if (player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) &&
                                            OptionGroupSingleton<TraitorOptions>.Instance.NeutralKillingStopsTraitor))
                {
                    return;
                }
            }

            var traitorPlayer = traitor.Player;
            if (traitorPlayer.Data.IsDead)
            {
                return;
            }

            var otherTraitors = Helpers.GetAlivePlayers()
                .Where(x => x.HasModifier<ToBecomeTraitorModifier>() && x != traitorPlayer).ToList();
            foreach (var faker in otherTraitors)
            {
                faker.RpcRemoveModifier<ToBecomeTraitorModifier>();
            }

            ToBecomeTraitorModifier.RpcSetTraitor(traitorPlayer);
        }
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        var player = @event.Player;

        if (!PlayerControl.LocalPlayer || player == null || (@event.NewRole is not ILoyalCrewmate loyal || loyal.CanBeTraitor))
        {
            return;
        }
        if (player.TryGetModifier<ToBecomeTraitorModifier>(out var traitorMod))
        {
            traitorMod.Clear();
        }
    }

    [RegisterEvent]
    public static void SetRoleHandler(SetRoleEvent @event)
    {
        var player = @event.Player;

        if (!PlayerControl.LocalPlayer || player == null ||
            (RoleManager.Instance.AllRoles.ToArray().First(x => x.Role == @event.Role) is not ILoyalCrewmate loyal ||
             loyal.CanBeTraitor))
        {
            return;
        }

        if (player.TryGetModifier<ToBecomeTraitorModifier>(out var traitorMod))
        {
            traitorMod.Clear();
        }
    }
}