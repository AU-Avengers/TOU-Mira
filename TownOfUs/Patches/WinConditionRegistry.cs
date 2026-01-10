using System.Collections.Generic;
using System.Linq;
using TownOfUs.Interfaces;

namespace TownOfUs.Patches;

/// <summary>
///     Registry for win conditions that can be evaluated during game flow.
///     Allows extension mods to register their own win conditions.
/// </summary>
public static class WinConditionRegistry
{
    private static readonly List<IWinCondition> Conditions = new();
    private static readonly object LockObject = new();

    /// <summary>
    ///     Gets a read-only list of all registered win conditions.
    /// </summary>
    public static IReadOnlyList<IWinCondition> RegisteredConditions
    {
        get
        {
            lock (LockObject)
            {
                return Conditions.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    ///     Registers a win condition. Conditions are automatically sorted by priority.
    /// </summary>
    /// <param name="condition">The win condition to register.</param>
    public static void Register(IWinCondition condition)
    {
        if (condition == null)
        {
            return;
        }

        lock (LockObject)
        {
            Conditions.Add(condition);
            Conditions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
    }

    /// <summary>
    ///     Evaluates all registered win conditions in priority order.
    ///     Returns true if a condition was met and triggered game over.
    /// </summary>
    /// <param name="instance">The game flow instance to check.</param>
    /// <returns>True if a win condition was met and game over was triggered.</returns>
    public static bool TryEvaluate(LogicGameFlowNormal instance)
    {
        IWinCondition[] conditionsCopy;
        lock (LockObject)
        {
            conditionsCopy = Conditions.ToArray();
        }

        foreach (var condition in conditionsCopy)
        {
            if (!condition.IsMet(instance))
            {
                continue;
            }

            condition.TriggerGameOver(instance);

            if (condition is IWinConditionWithBlocking block && block.BlocksOthers)
            {
                return true;
            }

            return true;
        }

        return false;
    }
}