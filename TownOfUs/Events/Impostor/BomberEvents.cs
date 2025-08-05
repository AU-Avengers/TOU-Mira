using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Impostor;

namespace TownOfUs.Events.Impostor;

public static class BomberEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            CustomButtonSingleton<BomberPlantButton>.Instance.IsFirstRound = true;
        }
        else
        {
            CustomButtonSingleton<BomberPlantButton>.Instance.IsFirstRound = false;
        }
    }
}