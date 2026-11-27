using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Events.Modifiers;

[RegisterEventContainer]
public static class ImmovableEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        ModifierUtils.GetActiveModifiers<ImmovableModifier>().Do(x => x.OnRoundStart());
    }
}