using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Impostor;

namespace TownOfUs.Events.Modifiers;

public static class CircumventEvents
{
    [RegisterEvent]
    public static void ExitVentEventHandler(ExitVentEvent @event)
    {
        var player = @event.Player;
        var vent = @event.Vent;

        if (vent == null || !player.TryGetModifier<CircumventModifier>(out var circumcisionMod))
        {
            return;
        }

        --circumcisionMod.VentsAvailable;
    }
}