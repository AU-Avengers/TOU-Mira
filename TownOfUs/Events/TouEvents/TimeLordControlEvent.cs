namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when Parasite starts controlling a target.
/// </summary>
public sealed class TimeLordParasiteControlEvent : TimeLordEvent
{
    /// <summary>
    /// The target being controlled.
    /// </summary>
    public PlayerControl Target { get; }

    /// <summary>
    /// The target's player ID.
    /// </summary>
    public byte TargetId { get; }

    public TimeLordParasiteControlEvent(PlayerControl parasite, PlayerControl target, float time)
        : base(parasite, time)
    {
        Target = target;
        TargetId = target.PlayerId;
    }
}

/// <summary>
/// Undo event for a Parasite control start during Time Lord rewind.
/// This will end control on the target if still active.
/// </summary>
public sealed class TimeLordParasiteControlUndoEvent : TimeLordUndoEvent
{
    public TimeLordParasiteControlUndoEvent(TimeLordParasiteControlEvent originalEvent)
        : base(originalEvent)
    {
    }
}

/// <summary>
/// Event fired when Puppeteer starts controlling a target.
/// </summary>
public sealed class TimeLordPuppeteerControlEvent : TimeLordEvent
{
    /// <summary>
    /// The target being controlled.
    /// </summary>
    public PlayerControl Target { get; }

    /// <summary>
    /// The target's player ID.
    /// </summary>
    public byte TargetId { get; }

    public TimeLordPuppeteerControlEvent(PlayerControl puppeteer, PlayerControl target, float time)
        : base(puppeteer, time)
    {
        Target = target;
        TargetId = target.PlayerId;
    }
}

/// <summary>
/// Undo event for a Puppeteer control start during Time Lord rewind.
/// This will end control on the target if still active.
/// </summary>
public sealed class TimeLordPuppeteerControlUndoEvent : TimeLordUndoEvent
{
    public TimeLordPuppeteerControlUndoEvent(TimeLordPuppeteerControlEvent originalEvent)
        : base(originalEvent)
    {
    }
}


