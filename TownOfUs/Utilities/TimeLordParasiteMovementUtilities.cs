using Rewired;
using UnityEngine;

namespace TownOfUs.Utilities;

/// <summary>
/// Shared movement utilities for Time Lord and Parasite roles.
/// These handle directional input and movement calculations.
/// </summary>
public static class TimeLordParasiteMovementUtilities
{
    private static void EnsureKeybindHasKey(MiraKeybind keybind)
    {
        if (keybind == null)
        {
            return;
        }

        if (keybind.DefaultKey == KeyboardKeyCode.None)
        {
            return;
        }

        if (keybind.RewiredInputAction == null)
        {
            return;
        }

        try
        {
            var map = KeybindUtils.GetActionElementMap(keybind.RewiredInputAction.id);
            if (map == null)
            {
                return;
            }

            if (map._keyboardKeyCode == KeyboardKeyCode.None)
            {
                map._keyboardKeyCode = keybind.DefaultKey;
            }
        }
        catch
        {
            // Ignore
        }
    }

    private static bool IsKeybindHeld(BaseKeybind keybind)
    {
        if (keybind == null)
        {
            return false;
        }

        try
        {
            if (keybind is MiraKeybind miraKeybind)
            {
                EnsureKeybindHasKey(miraKeybind);
            }

            return ReInput.players.GetPlayer(0).GetButton(keybind.Id);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets Parasite primary direction input for the controller's own movement.
    /// Uses TouKeybinds on keyboard, joystick/touch input on mobile, and controller axes on gamepad.
    /// </summary>
    public static Vector2 GetParasitePrimaryDirection()
    {
        var hudManager = HudManager.Instance;

        var x = 0f;
        var y = 0f;

        var controlType = ActiveInputManager.currentControlType;

        if (controlType != ActiveInputManager.InputType.Keyboard &&
            HudManager.InstanceExists && hudManager.joystick != null)
        {
            var vJoy = hudManager.joystick.DeltaL;
            return vJoy == Vector2.zero ? Vector2.zero : vJoy.normalized;
        }

        if (controlType is ActiveInputManager.InputType.Joystick)
        {
            x = ConsoleJoystick.player.GetAxis(2);
            y = ConsoleJoystick.player.GetAxis(3);
        }
        else
        {
            if (IsKeybindHeld(TouKeybinds.ParasitePrimaryRight))
            {
                x += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ParasitePrimaryLeft))
            {
                x -= 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ParasitePrimaryUp))
            {
                y += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ParasitePrimaryDown))
            {
                y -= 1f;
            }
        }

        var v = new Vector2(x, y);
        return v == Vector2.zero ? Vector2.zero : v.normalized;
    }

    /// <summary>
    /// Gets Parasite secondary direction input for the controlled target's movement.
    /// Uses TouKeybinds on keyboard, joystick/touch input on mobile, and controller axes on gamepad.
    /// </summary>
    public static Vector2 GetParasiteSecondaryDirection()
    {
        var hudManager = HudManager.Instance;

        var x = 0f;
        var y = 0f;

        var controlType = ActiveInputManager.currentControlType;

        if (controlType != ActiveInputManager.InputType.Keyboard &&
            HudManager.InstanceExists && hudManager.joystickR != null)
        {
            var vJoy = hudManager.joystickR.DeltaL;
            return vJoy == Vector2.zero ? Vector2.zero : vJoy.normalized;
        }

        if (controlType is ActiveInputManager.InputType.Joystick)
        {
            x = ConsoleJoystick.player.GetAxis(54);
            y = ConsoleJoystick.player.GetAxis(55);
        }
        else
        {
            if (IsKeybindHeld(TouKeybinds.ParasiteSecondaryRight))
            {
                x += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ParasiteSecondaryLeft))
            {
                x -= 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ParasiteSecondaryUp))
            {
                y += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ParasiteSecondaryDown))
            {
                y -= 1f;
            }
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