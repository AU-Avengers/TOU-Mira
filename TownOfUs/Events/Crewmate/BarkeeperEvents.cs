using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Modules.Components;

namespace TownOfUs.Events.Crewmate;

[RegisterEventContainer]
public static class BarkeeperEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        DrinkSpillComponent.Clear();
    }
}