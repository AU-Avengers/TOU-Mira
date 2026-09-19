using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;

namespace TownOfUs.Events.Modifiers;

public static class BaitEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Target.HasModifier<BaitModifier>() && @event.Target != @event.Source &&
            !MeetingHud.Instance && @event.DeadBody != null)
        {
            Coroutines.Start(BaitModifier.CoReportDelay(@event.Source, @event.Target));
        }
    }
}