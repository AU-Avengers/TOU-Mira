using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.WinConditions;

/// <summary>
///     Win condition for neutral roles.
///     Checks all neutral roles and triggers game over for the first one that meets its win condition.
///     Handles special case where Jester/Executioner/Doomsayer with Lover modifier should trigger Lovers win instead.
/// </summary>
public sealed class NeutralRoleWinCondition : IWinCondition
{
    /// <summary>
    ///     Priority 5 - executes before lovers (10) and crew/impostor.
    /// </summary>
    public int Priority => 5;

    /// <summary>
    ///     Checks if any neutral role win condition is met.
    /// </summary>
    public bool IsMet(LogicGameFlowNormal _)
    {
        return CustomRoleUtils.GetActiveRolesOfTeam(ModdedRoleTeams.Custom)
            .OrderBy(x => x.GetRoleAlignment())
            .Any(x => x is ITownOfUsRole r && r.WinConditionMet());
    }

    /// <summary>
    ///     Triggers the neutral game over, or lovers game over if applicable.
    /// </summary>
    public void TriggerGameOver(LogicGameFlowNormal _)
    {
        var winner = CustomRoleUtils.GetActiveRolesOfTeam(ModdedRoleTeams.Custom)
            .OrderBy(x => x.GetRoleAlignment())
            .FirstOrDefault(x => x is ITownOfUsRole r && r.WinConditionMet());

        if (winner == null)
        {
            return;
        }

        // Lovers + Jest/Exe/Doom combo fix
        // If the winner is Jester, Executioner, or Doomsayer and has Lover modifier,
        // check if we should trigger Lovers win instead
        if ((winner is JesterRole || winner is ExecutionerRole || winner is DoomsayerRole) &&
            winner.Player != null && winner.Player.HasModifier<LoverModifier>())
        {
            var loverWinners = ModifierUtils.GetActiveModifiers<LoverModifier>()
                .Where(x => x.Player != null && x.Player.Data != null && x.Player.HasModifier<LoverModifier>())
                .Select(x => x.Player!.Data)
                .Distinct()
                .ToArray();

            if (loverWinners.Length >= 2)
            {
                CustomGameOver.Trigger<LoverGameOver>(loverWinners);
                return;
            }
        }

        if (winner.Player != null)
        {
            CustomGameOver.Trigger<NeutralGameOver>([winner.Player.Data]);
        }
    }
}