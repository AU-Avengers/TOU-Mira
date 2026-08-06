using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class ScavengerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (!source.AmOwner || source.Data.Role is not ScavengerRole scavenger)
        {
            return;
        }

        scavenger.OnPlayerKilled(@event.Target);
    }
}