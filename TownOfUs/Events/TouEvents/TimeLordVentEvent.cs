namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a player enters a vent.
/// </summary>
public class TimeLordVentEnterEvent : TimeLordEvent
{
    /// <summary>
    /// The vent that was entered.
    /// </summary>
    public Vent Vent { get; }

    /// <summary>
    /// The vent ID.
    /// </summary>
    public int VentId { get; }

    public TimeLordVentEnterEvent(PlayerControl player, Vent vent, float time) : base(player, time)
    {
        Vent = vent;
        VentId = vent.Id;
    }
}

/// <summary>
/// Event fired when a player exits a vent.
/// </summary>
public class TimeLordVentExitEvent : TimeLordEvent
{
    /// <summary>
    /// The vent that was exited.
    /// </summary>
    public Vent Vent { get; }

    /// <summary>
    /// The vent ID.
    /// </summary>
    public int VentId { get; }

    public TimeLordVentExitEvent(PlayerControl player, Vent vent, float time) : base(player, time)
    {
        Vent = vent;
        VentId = vent.Id;
    }
}

/// <summary>
/// Event fired to undo a vent enter action during rewind.
/// </summary>
public class TimeLordVentEnterUndoEvent : TimeLordUndoEvent
{
    public TimeLordVentEnterUndoEvent(TimeLordVentEnterEvent originalEvent) : base(originalEvent)
    {
    }
}

/// <summary>
/// Event fired to undo a vent exit action during rewind.
/// </summary>
public class TimeLordVentExitUndoEvent : TimeLordUndoEvent
{
    public TimeLordVentExitUndoEvent(TimeLordVentExitEvent originalEvent) : base(originalEvent)
    {
    }
}