namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a player completes a task.
/// </summary>
public class TimeLordTaskCompleteEvent(PlayerControl player, PlayerTask task, float time) : TimeLordEvent(player, time)
{
    /// <summary>
    /// The task that was completed.
    /// </summary>
    public PlayerTask Task { get; } = task;

    /// <summary>
    /// The task ID.
    /// </summary>
    public uint TaskId { get; } = task.Id;
}

/// <summary>
/// Event fired to undo a task completion during rewind.
/// </summary>
public class TimeLordTaskCompleteUndoEvent(TimeLordTaskCompleteEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}