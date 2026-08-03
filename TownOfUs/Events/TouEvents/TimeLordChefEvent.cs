using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when Chef cooks a body.
/// </summary>
public class TimeLordChefCookEvent(PlayerControl chef, DeadBody body, PlatterType platterType, float time) : TimeLordEvent(chef, time)
{
    /// <summary>
    /// The body that was cooked.
    /// </summary>
    public DeadBody Body { get; } = body;

    /// <summary>
    /// The body ID (player ID).
    /// </summary>
    public byte BodyId { get; } = body.ParentId;

    /// <summary>
    /// The platter type the body was cooked into.
    /// </summary>
    public PlatterType PlatterType { get; } = platterType;
}

/// <summary>
/// Event fired when Chef serves a body to a player.
/// </summary>
public class TimeLordChefServeEvent(PlayerControl chef, PlayerControl target, byte bodyId, PlatterType platterType, float time) : TimeLordEvent(chef, time)
{
    /// <summary>
    /// The player who was served.
    /// </summary>
    public PlayerControl Target { get; } = target;

    /// <summary>
    /// The target's player ID.
    /// </summary>
    public byte TargetId { get; } = target.PlayerId;

    /// <summary>
    /// The body ID that was served.
    /// </summary>
    public byte BodyId { get; } = bodyId;

    /// <summary>
    /// The platter type that was served.
    /// </summary>
    public PlatterType PlatterType { get; } = platterType;
}

/// <summary>
/// Event fired to undo a Chef cook action during rewind (restore the body).
/// </summary>
public class TimeLordChefCookUndoEvent(TimeLordChefCookEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}

/// <summary>
/// Event fired to undo a Chef serve action during rewind (remove the served modifier).
/// </summary>
public class TimeLordChefServeUndoEvent(TimeLordChefServeEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}