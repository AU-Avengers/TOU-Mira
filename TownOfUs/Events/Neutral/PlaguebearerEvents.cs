using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Hud;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class PlaguebearerEvents
{
    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Target == null)
        {
            return;
        }

        PlaguebearerRole.CheckInfected(@event.Target.Object, @event.Reporter);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        PlaguebearerRole.CheckInfected(@event.Source, @event.Target);
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (@event.Button is PlaguebearerInfectButton)
        {
            return;
        }

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        PlaguebearerRole.RpcCheckInfected(source, target);
    }
}