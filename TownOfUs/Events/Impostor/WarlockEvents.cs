using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class WarlockEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!@event.Source.AmOwner || !@event.Source.IsRole<WarlockRole>())
        {
            return;
        }

        var button = CustomButtonSingleton<WarlockKillButton>.Instance;
        if (button.BurstActive)
        {
            ++button.Kills;
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return; // Only run when round starts.
        }

        var button = CustomButtonSingleton<WarlockKillButton>.Instance;
        button.Charge = 0f;
        button.BurstActive = false;
        button.ResetCooldownAndOrEffect();
    }
}