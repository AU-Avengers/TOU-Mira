namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a player is killed.
/// </summary>
public class TimeLordKillEvent : TimeLordEvent
{
    /// <summary>
    /// The victim who was killed.
    /// </summary>
    public PlayerControl Victim { get; }

    /// <summary>
    /// The victim's player ID.
    /// </summary>
    public byte VictimId { get; }

    public TimeLordKillEvent(PlayerControl killer, PlayerControl victim, float time) : base(killer, time)
    {
        Victim = victim;
        VictimId = victim.PlayerId;
    }
}

/// <summary>
/// Event fired to undo a kill during rewind (revive the player).
/// </summary>
public class TimeLordKillUndoEvent : TimeLordUndoEvent
{
    public TimeLordKillUndoEvent(TimeLordKillEvent originalEvent) : base(originalEvent)
    {
    }
}