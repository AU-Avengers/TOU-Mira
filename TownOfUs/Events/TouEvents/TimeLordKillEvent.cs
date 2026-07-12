namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a player is killed.
/// </summary>
public class TimeLordKillEvent(PlayerControl killer, PlayerControl victim, float time) : TimeLordEvent(killer, time)
{
    /// <summary>
    /// The victim who was killed.
    /// </summary>
    public PlayerControl Victim { get; } = victim;

    /// <summary>
    /// The victim's player ID.
    /// </summary>
    public byte VictimId { get; } = victim.PlayerId;
}

/// <summary>
/// Event fired to undo a kill during rewind (revive the player).
/// </summary>
public class TimeLordKillUndoEvent(TimeLordKillEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}