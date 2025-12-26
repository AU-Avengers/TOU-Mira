using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Impostor;

namespace TownOfUs.Events.Impostor;

public static class ParasiteEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var button = CustomButtonSingleton<ParasiteOvertakeButton>.Instance;
        button.ResetCooldownAndOrEffect();
    }
}