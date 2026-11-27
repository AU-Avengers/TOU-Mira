using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;

namespace TownOfUs.Events.Crewmate;

[RegisterEventContainer]
public static class ScientistEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.HasModifier<ScientistModifier>() && @event.Player.AmOwner)
        {
            ScientistModifier.OnTaskComplete();
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return; // Never run when round starts.
        }

        if (PlayerControl.LocalPlayer.HasModifier<ScientistModifier>())
        {
            ScientistModifier.OnRoundStart();
        }
    }
}