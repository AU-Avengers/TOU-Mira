namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a kill cooldown changes.
/// </summary>
public class TimeLordKillCooldownEvent : TimeLordEvent
{
    /// <summary>
    /// The kill cooldown value before the change.
    /// </summary>
    public float CooldownBefore { get; }

    /// <summary>
    /// The kill cooldown value after the change.
    /// </summary>
    public float CooldownAfter { get; }

    public TimeLordKillCooldownEvent(PlayerControl player, float cooldownBefore, float cooldownAfter, float time) 
        : base(player, time)
    {
        CooldownBefore = cooldownBefore;
        CooldownAfter = cooldownAfter;
    }
}

/// <summary>
/// Event fired to undo a kill cooldown change during rewind (restore the previous cooldown).
/// </summary>
public class TimeLordKillCooldownUndoEvent : TimeLordUndoEvent
{
    public TimeLordKillCooldownUndoEvent(TimeLordKillCooldownEvent originalEvent) : base(originalEvent)
    {
    }
}