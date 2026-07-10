using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Events.Modifiers;

public static class ShyEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent _)
    {
        ModifierUtils.GetActiveModifiers<ShyModifier>().Do(x => x.OnRoundStart());
    }

    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Target.HasModifier<ShyModifier>())
        {
            ShyModifier.SetVisibility(@event.Target, 1f);
        }
    }
}