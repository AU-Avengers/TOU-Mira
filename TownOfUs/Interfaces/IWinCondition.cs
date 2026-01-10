namespace TownOfUs.Interfaces;

/// <summary>
///     Interface for win conditions that can be evaluated during game flow.
///     Lower priority executes first (neutral > lovers > crew > impostor etc).
/// </summary>
public interface IWinCondition
{
    /// <summary>
    ///     Priority determines execution order. Lower values execute first.
    ///     Typical priorities: Neutral (5), Lovers (10), Crew (20), Impostor (30).
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Returns true if this condition wants to end the game.
    /// </summary>
    /// <param name="gameFlow">The game flow instance to check.</param>
    /// <returns>True if the win condition is met.</returns>
    bool IsMet(LogicGameFlowNormal gameFlow);

    /// <summary>
    ///     Called if IsMet returned true. Should trigger the appropriate game over.
    /// </summary>
    /// <param name="gameFlow">The game flow instance.</param>
    void TriggerGameOver(LogicGameFlowNormal gameFlow);
}

/// <summary>
///     Optional interface for win conditions that can block lower priority conditions.
/// </summary>
public interface IWinConditionWithBlocking
{
    /// <summary>
    ///     If true, no lower priority conditions will be checked after this one triggers.
    /// </summary>
    bool BlocksOthers { get; }
}

