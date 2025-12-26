using MiraAPI.Events;

namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Base event for Time Lord actions. These events are recorded during normal gameplay
/// and can be undone during rewind.
/// </summary>
public abstract class TimeLordEvent : MiraEvent
{
    /// <summary>
    /// The player who performed the action.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// The time when the action occurred (Time.time).
    /// </summary>
    public float Time { get; }

    protected TimeLordEvent(PlayerControl player, float time)
    {
        Player = player;
        Time = time;
    }
}

/// <summary>
/// Base event for undoing Time Lord actions during rewind.
/// </summary>
public abstract class TimeLordUndoEvent : MiraEvent
{
    /// <summary>
    /// The original event that is being undone.
    /// </summary>
    public TimeLordEvent OriginalEvent { get; }

    protected TimeLordUndoEvent(TimeLordEvent originalEvent)
    {
        OriginalEvent = originalEvent;
    }
}