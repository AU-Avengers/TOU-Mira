using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Utilities;

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
        if (@event.TriggeredByIntro || !PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }
        var crewpostor = ModifierUtils.GetActiveModifiers<CrewpostorModifier>()
            .FirstOrDefault(x => !x.Player.HasDied() && x.Player.IsCrewmate());
        var alives = Helpers.GetAlivePlayers().ToList();
        if (crewpostor != null)
        {
            if (alives.Any(x => x.IsImpostor()))
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
}