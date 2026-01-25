using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;

namespace TownOfUs.Events.Misc;

public static class ScatterEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        //Error($"ScatterEvents - RoundStartHandler");

        ModifierUtils.GetActiveModifiers<ScatterModifier>().Do(x => x.OnRoundStart());
    }
}