using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class OfficerEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return;
        }

        var shootButton = CustomButtonSingleton<OfficerShootButton>.Instance;
        shootButton.TotalBullets = -1;
        shootButton.RoundsBeforeReset = 0;
        shootButton.LoadedBullets = 0;
    }
}
