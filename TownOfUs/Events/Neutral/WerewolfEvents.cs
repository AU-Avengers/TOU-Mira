using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class WerewolfEvents
{
    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!@event.IsVent)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var vent = @event.Usable as Vent;

        if (vent == null)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.Data.Role is WerewolfRole werewolf &&
            !werewolf.Rampaging)
        {
            @event.Cancel();
        }
    }
}