using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class ScavengerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source is not { AmOwner: true, Data.Role: ScavengerRole scavenger })
        {
            return;
        }

        scavenger.OnPlayerKilled(@event.Target);
    }
}