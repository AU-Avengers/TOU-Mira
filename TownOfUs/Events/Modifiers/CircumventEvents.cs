using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Patches;

namespace TownOfUs.Events.Modifiers;

public static class CircumventEvents
{
    [RegisterEvent]
    public static void EnterVentEventHandler(EnterVentEvent @event)
    {
        var player = @event.Player;
        var vent = @event.Vent;

        if (vent == null || !player.TryGetModifier<CircumventModifier>(out var circumcisionMod))
        {
            return;
        }

        circumcisionMod.InVent = true;
        --circumcisionMod.VentsAvailable;
    }
    [RegisterEvent]
    public static void ExitVentEventHandler(ExitVentEvent @event)
    {
        var player = @event.Player;
        var vent = @event.Vent;

        if (vent == null || !player.TryGetModifier<CircumventModifier>(out var circumcisionMod))
        {
            return;
        }
        circumcisionMod.InVent = false;
    }

    public static void RoundStartEvent(RoundStartEvent @event)
    {
        foreach (var circumcisionMod in ModifierUtils.GetActiveModifiers<CircumventModifier>())
        {
            circumcisionMod.InVent = false;
        }
    }
}