using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;

namespace TownOfUs.Patches.WinConditions;

/// <summary>
///     Win condition for Lovers modifier.
///     Triggers when there are 3 or fewer players alive and 2 are lovers.
/// </summary>
public sealed class LoversWinCondition : IWinCondition, IWinConditionWithBlocking
{
    /// <summary>
    ///     Priority 10 - executes after neutral roles (5) but before crew/impostor.
    /// </summary>
    public int Priority => 10;

    /// <summary>
    ///     Lovers win blocks other conditions.
    /// </summary>
    public bool BlocksOthers => true;

    /// <summary>
    ///     Checks if the lovers win condition is met.
    /// </summary>
    public bool IsMet(LogicGameFlowNormal _)
    {
        var lovers = ModifierUtils.GetActiveModifiers<LoverModifier>().ToArray();
        return LoverModifier.WinConditionMet(lovers);
    }

    /// <summary>
    ///     Triggers the lovers game over.
    /// </summary>
    public void TriggerGameOver(LogicGameFlowNormal _)
    {
        var winners = ModifierUtils.GetActiveModifiers<LoverModifier>()
            .Where(x => x?.Player?.Data != null)
            .Select(x => x.Player!.Data)
            .ToArray();

        CustomGameOver.Trigger<LoverGameOver>(winners);
    }
}