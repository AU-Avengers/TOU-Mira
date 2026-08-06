namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a player enters a vent.
/// </summary>
public class TimeLordVentEnterEvent(PlayerControl player, Vent vent, float time) : TimeLordEvent(player, time)
{
    /// <summary>
    /// The vent that was entered.
    /// </summary>
    public Vent Vent { get; } = vent;

    /// <summary>
    /// The vent ID.
    /// </summary>
    public int VentId { get; } = vent.Id;
}

/// <summary>
/// Event fired when a player exits a vent.
/// </summary>
public class TimeLordVentExitEvent(PlayerControl player, Vent vent, float time) : TimeLordEvent(player, time)
{
    /// <summary>
    /// The vent that was exited.
    /// </summary>
    public Vent Vent { get; } = vent;

    /// <summary>
    /// The vent ID.
    /// </summary>
    public int VentId { get; } = vent.Id;
}

/// <summary>
/// Event fired to undo a vent enter action during rewind.
/// </summary>
public class TimeLordVentEnterUndoEvent(TimeLordVentEnterEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}

/// <summary>
/// Event fired to undo a vent exit action during rewind.
/// </summary>
public class TimeLordVentExitUndoEvent(TimeLordVentExitEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}