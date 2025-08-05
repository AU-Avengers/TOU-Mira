using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Impostor;

namespace TownOfUs.Events.Impostor;

public static class BomberEvents
{
    [RegisterEvent(10000)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }
        CustomButtonSingleton<BomberPlantButton>.Instance.Usable = true;
    }
}