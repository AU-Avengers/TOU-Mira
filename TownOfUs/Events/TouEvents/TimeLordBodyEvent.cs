using TownOfUs.Modules.TimeLord;
using UnityEngine;

namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a body is cleaned (hidden).
/// </summary>
public class TimeLordBodyCleanedEvent : TimeLordEvent
{
    /// <summary>
    /// The body that was cleaned.
    /// </summary>
    public DeadBody Body { get; }

    /// <summary>
    /// The body ID (player ID).
    /// </summary>
    public byte BodyId { get; }

    /// <summary>
    /// The position where the body was cleaned.
    /// </summary>
    public Vector3 Position { get; }

    /// <summary>
    /// The source of the cleaning (Janitor, Rotting, etc.).
    /// </summary>
    public TimeLordBodyManager.CleanedBodySource Source { get; }

    public TimeLordBodyCleanedEvent(PlayerControl player, DeadBody body, Vector3 position, 
        TimeLordBodyManager.CleanedBodySource source, float time) : base(player, time)
    {
        Body = body;
        BodyId = body.ParentId;
        Position = position;
        Source = source;
    }
}

/// <summary>
/// Event fired to undo a body cleaning during rewind (restore the body).
/// </summary>
public class TimeLordBodyCleanedUndoEvent : TimeLordUndoEvent
{
    public TimeLordBodyCleanedUndoEvent(TimeLordBodyCleanedEvent originalEvent) : base(originalEvent)
    {
    }
}