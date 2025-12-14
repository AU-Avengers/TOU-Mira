using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.HnsGame.Crewmate;

namespace TownOfUs.Events.HnsModifiers;

public static class HnsTransporterEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!@event.Target.HasModifier<HnsTransporterModifier>() || @event.Target == @event.Source ||
            MeetingHud.Instance)
        {
            return;
        }

        if (@event.Source.AmOwner)
        {
            HnsTransporterModifier.RpcTransportSeeker(@event.Target, @event.Source);
        }
    }
}