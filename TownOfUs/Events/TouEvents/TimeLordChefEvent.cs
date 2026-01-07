using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when Chef cooks a body.
/// </summary>
public class TimeLordChefCookEvent : TimeLordEvent
{
    /// <summary>
    /// The body that was cooked.
    /// </summary>
    public DeadBody Body { get; }

    /// <summary>
    /// The body ID (player ID).
    /// </summary>
    public byte BodyId { get; }

    /// <summary>
    /// The platter type the body was cooked into.
    /// </summary>
    public PlatterType PlatterType { get; }

    public TimeLordChefCookEvent(PlayerControl chef, DeadBody body, PlatterType platterType, float time) 
        : base(chef, time)
    {
        Body = body;
        BodyId = body.ParentId;
        PlatterType = platterType;
    }
}

/// <summary>
/// Event fired when Chef serves a body to a player.
/// </summary>
public class TimeLordChefServeEvent : TimeLordEvent
{
    /// <summary>
    /// The player who was served.
    /// </summary>
    public PlayerControl Target { get; }

    /// <summary>
    /// The target's player ID.
    /// </summary>
    public byte TargetId { get; }

    /// <summary>
    /// The body ID that was served.
    /// </summary>
    public byte BodyId { get; }

    /// <summary>
    /// The platter type that was served.
    /// </summary>
    public PlatterType PlatterType { get; }

    public TimeLordChefServeEvent(PlayerControl chef, PlayerControl target, byte bodyId, PlatterType platterType, float time) 
        : base(chef, time)
    {
        Target = target;
        TargetId = target.PlayerId;
        BodyId = bodyId;
        PlatterType = platterType;
    }
}

/// <summary>
/// Event fired to undo a Chef cook action during rewind (restore the body).
/// </summary>
public class TimeLordChefCookUndoEvent : TimeLordUndoEvent
{
    public TimeLordChefCookUndoEvent(TimeLordChefCookEvent originalEvent) : base(originalEvent)
    {
    }
}

/// <summary>
/// Event fired to undo a Chef serve action during rewind (remove the served modifier).
/// </summary>
public class TimeLordChefServeUndoEvent : TimeLordUndoEvent
{
    public TimeLordChefServeUndoEvent(TimeLordChefServeEvent originalEvent) : base(originalEvent)
    {
    }
}