using AmongUs.Data;
using Rewired;
using UnityEngine;

namespace TownOfUs.Utilities;

/// <summary>
/// Shared movement utilities for Time Lord, Parasite, Puppeteer, and any other controlling roles.
/// These handle directional input and movement calculations.
/// </summary>
public static class AdvancedMovementUtilities
{
    private static int _controlledMovementDepth;
    public static bool IsApplyingControlledMovement => _controlledMovementDepth > 0;

    // Check Parasite's implementation of this to see how the joystick should be added in.
    public static VirtualJoystick MobileJoystickR { get; set; }

    public static void CreateMobileJoystick()
    {
        if (MobileJoystickR != null) return;
        if (!HudManager.InstanceExists) return;
        var hudManager = HudManager.Instance;
        MonoBehaviour monoBehaviour2 = UnityEngine.Object.Instantiate(hudManager.RightVJoystick);
        if (monoBehaviour2 != null)
        {
            monoBehaviour2.transform.SetParent(hudManager.transform, false);
            MobileJoystickR = monoBehaviour2.GetComponent<VirtualJoystick>();
            MobileJoystickR.ToggleVisuals(false);
        }
    }

    public static void ResizeMobileJoystick()
    {
        if (MobileJoystickR == null) return;
        if (!HudManager.InstanceExists) return;
        MobileJoystickR.ToggleVisuals(true);
        var size = DataManager.Settings.Input.TouchJoystickSize;
        var num = Mathf.Lerp(0.65f, 1.1f, FloatRange.ReverseLerp(size, 0.5f, 1.5f));
        HudManager.Instance.SetVirtualJoystickSize(MobileJoystickR, size, new Vector2(num + 0.4f, num - 0.6f));
        MobileJoystickR.ToggleVisuals(false);
    }
    private static void EnsureKeybindHasKey(MiraKeybind keybind)
    {
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
    /// Gets the regular direction input for the controller's own movement.
    /// Uses TouKeybinds on keyboard, joystick/touch input on mobile, and controller axes on gamepad.
    /// </summary>
    public static Vector2 GetRegularDirection()
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
            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryRight) || IsKeybindHeld(TouKeybinds.ControlRoleSecondaryRight))
            {
                x += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryLeft) || IsKeybindHeld(TouKeybinds.ControlRoleSecondaryLeft))
            {
                x -= 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryUp) || IsKeybindHeld(TouKeybinds.ControlRoleSecondaryUp))
            {
                y += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryDown) || IsKeybindHeld(TouKeybinds.ControlRoleSecondaryDown))
            {
                y -= 1f;
            }
        }

        var v = new Vector2(x, y);
        return v == Vector2.zero ? Vector2.zero : v.normalized;
    }

    /// <summary>
    /// Gets Parasite primary direction input for the controller's own movement.
    /// Uses TouKeybinds on keyboard, joystick/touch input on mobile, and controller axes on gamepad.
    /// </summary>
    public static Vector2 GetControllerPrimaryDirection()
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
            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryRight))
            {
                x += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryLeft))
            {
                x -= 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryUp))
            {
                y += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRolePrimaryDown))
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
    public static Vector2 GetControllerSecondaryDirection()
    {
        var hudManager = HudManager.Instance;

        var x = 0f;
        var y = 0f;

        var controlType = ActiveInputManager.currentControlType;

        if (controlType != ActiveInputManager.InputType.Keyboard &&
            HudManager.InstanceExists && hudManager.joystick != null &&
            MobileJoystickR != null)
        {
            var vJoy = MobileJoystickR.DeltaR;
            return vJoy == Vector2.zero ? Vector2.zero : vJoy.normalized;
        }

        if (controlType is ActiveInputManager.InputType.Joystick)
        {
            x = ConsoleJoystick.player.GetAxis(54);
            y = ConsoleJoystick.player.GetAxis(55);
        }
        else
        {
            if (IsKeybindHeld(TouKeybinds.ControlRoleSecondaryRight))
            {
                x += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRoleSecondaryLeft))
            {
                x -= 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRoleSecondaryUp))
            {
                y += 1f;
            }

            if (IsKeybindHeld(TouKeybinds.ControlRoleSecondaryDown))
            {
                y -= 1f;
            }
        }

        var v = new Vector2(x, y);
        return v == Vector2.zero ? Vector2.zero : v.normalized;
    }

    /// <summary>
    /// Applies movement with flipped directions for Time Lord rewind.
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
    /// Applies normal movement for Parasite/Puppeteer control.
    /// Handles animation and velocity setting.
    /// </summary>
    public static void ApplyControlledMovement(PlayerPhysics physics, Vector2 direction, bool stopIfZero = false)
    {
        if (physics == null || physics.myPlayer == null)
        {
            return;
        }

        _controlledMovementDepth++;
        try
        {
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
        finally
        {
            _controlledMovementDepth = Mathf.Max(0, _controlledMovementDepth - 1);
        }
    }

    /// <summary>
    /// Applies injected movement with position, velocity, and direction for Parasite/Puppeteer control.
    /// </summary>
    public static void ApplyInjectedMovement(PlayerPhysics physics, Vector2 targetPosition, Vector2 targetVelocity, Vector2 direction)
    {
        if (physics == null || physics.myPlayer == null)
        {
            return;
        }

        _controlledMovementDepth++;
        try
        {
            physics.HandleAnimation(physics.myPlayer.Data.IsDead);

            var currentPos = physics.body != null ? physics.body.position : (Vector2)physics.myPlayer.transform.position;
            var delta = targetPosition - currentPos;
            const float positionSnapThreshold = 0.5f;

            if (delta.sqrMagnitude <= 0.0001f * 0.0001f || delta.magnitude > positionSnapThreshold)
            {
                if (physics.body != null)
                {
                    physics.body.position = targetPosition;
                }
                physics.myPlayer.transform.position = targetPosition;
            }
            else
            {
                const float positionLerpSpeed = 60.0f;
                var lerpFactor = Mathf.Clamp01(Time.fixedDeltaTime * positionLerpSpeed);
                var smoothedPos = Vector2.Lerp(currentPos, targetPosition, lerpFactor);
                if (physics.body != null)
                {
                    physics.body.position = smoothedPos;
                }
                physics.myPlayer.transform.position = smoothedPos;
            }


            physics.SetNormalizedVelocity(direction);

            if (physics.body != null && direction == Vector2.zero)
            {
                physics.body.velocity = Vector2.zero;
            }
        }
        finally
        {
            _controlledMovementDepth = Mathf.Max(0, _controlledMovementDepth - 1);
        }
    }
}