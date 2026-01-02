using TownOfUs.Modules.TimeLord;

namespace TownOfUs.Utilities;

/// <summary>
/// Shared helpers for determining whether a player is currently in a traversal/animation state
/// where targeting them would be unreliable (e.g. ladder/zipline/moving platform/vent transitions).
/// </summary>
public static class PlayerTargetingStateUtils
{
    /// <summary>
    /// Returns true if the player is in a state like gap pad (moving platform), walking to vent,
    /// ladder, zipline, or any other "invisible" transition animation.
    /// </summary>
    public static bool IsInTargetingAnimState(this PlayerControl? player)
    {
        if (player == null)
        {
            return false;
        }

        if ( player.walkingToVent)
        {
            return true;
        }

        if (player.onLadder)
        {
            return true;
        }

        if (player.inMovingPlat)
        {
            return true;
        }

        if (TimeLordAnimationUtilities.IsInInvisibleAnimation(player))
        {
            return true;
        }

        return false;
    }
}