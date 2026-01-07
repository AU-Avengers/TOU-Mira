namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a player completes a task.
/// </summary>
public class TimeLordTaskCompleteEvent : TimeLordEvent
{
    /// <summary>
    /// The task that was completed.
    /// </summary>
    public PlayerTask Task { get; }

    /// <summary>
    /// The task ID.
    /// </summary>
    public uint TaskId { get; }

    public TimeLordTaskCompleteEvent(PlayerControl player, PlayerTask task, float time) : base(player, time)
    {
        Task = task;
        TaskId = task.Id;
    }
}

/// <summary>
/// Event fired to undo a task completion during rewind.
/// </summary>
public class TimeLordTaskCompleteUndoEvent : TimeLordUndoEvent
{
    public TimeLordTaskCompleteUndoEvent(TimeLordTaskCompleteEvent originalEvent) : base(originalEvent)
    {
    }
}