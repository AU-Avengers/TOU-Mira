using TownOfUs.Modules.TimeLord;
using UnityEngine;

namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a body is cleaned (hidden).
/// </summary>
public class TimeLordBodyCleanedEvent(PlayerControl player, DeadBody body, Vector3 position,
    TimeLordBodyManager.CleanedBodySource source, float time) : TimeLordEvent(player, time)
{
    /// <summary>
    /// The body that was cleaned.
    /// </summary>
    public DeadBody Body { get; } = body;

    /// <summary>
    /// The body ID (player ID).
    /// </summary>
    public byte BodyId { get; } = body.ParentId;

    /// <summary>
    /// The position where the body was cleaned.
    /// </summary>
    public Vector3 Position { get; } = position;

    /// <summary>
    /// The source of the cleaning (Janitor, Rotting, etc.).
    /// </summary>
    public TimeLordBodyManager.CleanedBodySource Source { get; } = source;
}

/// <summary>
/// Event fired to undo a body cleaning during rewind (restore the body).
/// </summary>
public class TimeLordBodyCleanedUndoEvent(TimeLordBodyCleanedEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}