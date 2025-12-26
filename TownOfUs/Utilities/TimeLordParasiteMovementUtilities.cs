using UnityEngine;

namespace TownOfUs.Utilities;

/// <summary>
/// Shared movement utilities for Time Lord and Parasite roles.
/// These handle directional input and movement calculations.
/// </summary>
public static class TimeLordParasiteMovementUtilities
{
    /// <summary>
    /// Gets the WASD direction input. Used by Parasite when controlling a player.
    /// </summary>
    public static Vector2 GetWasdDirection()
    {
        var x = 0f;
        var y = 0f;

        if (Input.GetKey(KeyCode.D))
        {
            x += 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            y += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            y -= 1f;
        }

        var v = new Vector2(x, y);
        return v == Vector2.zero ? Vector2.zero : v.normalized;
    }

    /// <summary>
    /// Gets the arrow key direction input. Used by Parasite when moving independently.
    /// </summary>
    public static Vector2 GetArrowDirection()
    {
        var x = 0f;
        var y = 0f;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            x += 1f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            y += 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            y -= 1f;
        }

        var v = new Vector2(x, y);
        return v == Vector2.zero ? Vector2.zero : v.normalized;
    }

    /// <summary>
    /// Applies movement with flipped direction for Time Lord rewind.
    /// The direction is negated to simulate backwards movement.
    /// </summary>
    public static void ApplyRewindMovement(PlayerPhysics physics, Vector2 targetPosition, Vector2 currentPosition, bool isLadder)
    {
        var delta = targetPosition - currentPosition;
        const float idleEpsilon = 0.0005f;

        if (delta.sqrMagnitude <= idleEpsilon * idleEpsilon)
        {
            physics.HandleAnimation(physics.myPlayer.Data.IsDead);
            physics.SetNormalizedVelocity(Vector2.zero);
            if (physics.body != null)
            {
                physics.body.velocity = Vector2.zero;
            }
            return;
        }

        var dt = Mathf.Max(Time.fixedDeltaTime, 0.001f);
        var desiredVel = delta / dt;
        var dir = desiredVel.normalized;

        physics.HandleAnimation(physics.myPlayer.Data.IsDead);
        
        if (isLadder)
        {
            physics.SetNormalizedVelocity(Vector2.zero);
        }
        else
        {
            // Flip direction for rewind (backwards movement)
            physics.SetNormalizedVelocity(-dir);
        }

        if (physics.body != null)
        {
            physics.body.velocity = desiredVel;
        }
    }

    /// <summary>
    /// Applies normal movement for Parasite control.
    /// Handles animation and velocity setting.
    /// </summary>
    public static void ApplyParasiteMovement(PlayerPhysics physics, Vector2 direction, bool stopIfZero = false)
    {
        if (physics == null || physics.myPlayer == null)
        {
            return;
        }

        physics.HandleAnimation(physics.myPlayer.Data.IsDead);

        if (stopIfZero && direction == Vector2.zero)
        {
            physics.SetNormalizedVelocity(Vector2.zero);
            if (physics.body != null)
            {
                physics.body.velocity = Vector2.zero;
            }
            return;
        }

        physics.SetNormalizedVelocity(direction);

        if (direction == Vector2.zero && physics.body != null)
        {
            physics.body.velocity = Vector2.zero;
        }
    }
}