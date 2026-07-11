namespace TownOfUs.Events.TouEvents;

/// <summary>
/// Event fired when a kill cooldown changes.
/// </summary>
public class TimeLordKillCooldownEvent(PlayerControl player, float cooldownBefore, float cooldownAfter, float time) : TimeLordEvent(player, time)
{
    /// <summary>
    /// The kill cooldown value before the change.
    /// </summary>
    public float CooldownBefore { get; } = cooldownBefore;

    /// <summary>
    /// The kill cooldown value after the change.
    /// </summary>
    public float CooldownAfter { get; } = cooldownAfter;
}

/// <summary>
/// Event fired to undo a kill cooldown change during rewind (restore the previous cooldown).
/// </summary>
public class TimeLordKillCooldownUndoEvent(TimeLordKillCooldownEvent originalEvent) : TimeLordUndoEvent(originalEvent)
{
}