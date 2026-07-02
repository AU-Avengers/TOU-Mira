using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Modifiers;

public static class BaitEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Target.HasModifier<BaitModifier>() && @event.Target != @event.Source &&
            !@event.Source.IsRole<SoulCollectorRole>() &&
            !MeetingHud.Instance)
        {
            AmongUsClient.Instance.StartCoroutine(BaitModifier.CoReportDelay(@event.Source, @event.Target));
        }
    }
}