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

        if (@event.Usable.TryCast<Vent>() == null)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.Data.Role is WerewolfRole { Rampaging: false })
        {
            @event.Cancel();
        }
    }
}