using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Events.Modifiers;

[RegisterEventContainer]
public static class SatelliteEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return; // Never run when round starts.
        }

        ModifierUtils.GetActiveModifiers<SatelliteModifier>().Do(x => x.OnRoundStart());
    }
}