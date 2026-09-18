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
        if (!source || !source.AmOwner || !source.IsRole<ScavengerRole>())
        {
            return;
        }

        var scavenger = source.GetRole<ScavengerRole>();
        if (scavenger == null)
        {
            return;
        }

        scavenger.OnPlayerKilled(@event.Target);
    }
}