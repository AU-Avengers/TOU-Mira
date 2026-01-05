using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class TrapperEvents
{
    private static int ActiveTrapTaskCount;
    private static uint LastTrapUseTaskId = uint.MaxValue;

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            VentTrapSystem.ClearAll();
            ActiveTrapTaskCount = 0;
            LastTrapUseTaskId = uint.MaxValue;
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not TrapperRole)
        {
            return;
        }

        var options = OptionGroupSingleton<TrapperOptions>.Instance;
        if (!options.GetMoreFromTasks || options.TasksUntilMoreTraps <= 0)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastTrapUseTaskId)
        {
            ++ActiveTrapTaskCount;
            LastTrapUseTaskId = @event.Task.Id;
        }

        var button = CustomButtonSingleton<TrapperTrapButton>.Instance;
        if (button.LimitedUses && options.TasksUntilMoreTraps <= ActiveTrapTaskCount)
        {
            ++button.UsesLeft;
            button.SetUses(button.UsesLeft);
            ActiveTrapTaskCount = 0;
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        // “Rounds” advance on meeting resolution; match Plumber’s pattern.
        VentTrapSystem.DecrementRoundsAndRemoveExpired();
    }
}