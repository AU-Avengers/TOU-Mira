using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;

namespace TownOfUs.Events.Modifiers;

[RegisterEventContainer]
public static class TaskmasterEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return; // Only run when round starts.
        }

        ModifierUtils.GetActiveModifiers<TaskmasterModifier>().Do(x => x.OnRoundStart());
    }
}