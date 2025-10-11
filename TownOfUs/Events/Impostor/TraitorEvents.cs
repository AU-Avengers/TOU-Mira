using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Impostor;

public static class TraitorEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro || !PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }

        var traitor = ModifierUtils.GetActiveModifiers<ToBecomeTraitorModifier>()
            .Where(x => !x.Player.HasDied() && x.Player.IsCrewmate()).Random();
        if (traitor != null)
        {
            var alives = Helpers.GetAlivePlayers().ToList();
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