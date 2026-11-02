using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Networking.Rpc;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking.ControlSystem;
using TownOfUs.Utilities.ControlSystem;
using UnityEngine;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch]
public static class ControlMovementPatches
{
    public static Vector2 GetPrimaryDirection() => AdvancedMovementUtilities.GetControllerPrimaryDirection();

    public static Vector2 GetSecondaryDirection() => AdvancedMovementUtilities.GetControllerSecondaryDirection();

    public static Vector2 GetNormalDirection() => AdvancedMovementUtilities.GetRegularDirection();

    private const float MovementChangeEpsilonSqr = 0.0001f * 0.0001f;
    private const float MovementKeepAliveSeconds = 0.03f;

    private static readonly Dictionary<byte, Vector2> _lastSentDir = [];
    private static readonly Dictionary<byte, Vector2> _lastSentPos = [];
    private static readonly Dictionary<byte, Vector2> _lastSentVel = [];
    private static readonly Dictionary<byte, float> _lastSentDirAt = [];
    private static readonly Dictionary<byte, float> _lastSentStateAt = [];

    private static void SendControlledInputIfNeeded(byte controlledId, Vector2 dir, Vector2 position, bool inAnim, Vector2 velocity)
    {
        if (!ShouldSendVectors(
                controlledId, _lastSentStateAt,
                (position, _lastSentPos),
                (velocity, _lastSentVel)) ||
            !ShouldSendVectors(
                controlledId, _lastSentDirAt,
                (dir, _lastSentDir)))
        {
            return;
        }

        Rpc<FullControlInputUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new FullControlInputPacket(controlledId, dir, position, inAnim, velocity));
    }

    private static void SendControlledDirectionIfNeeded(byte controlledId, bool fromVictim, Vector2 dir)
    {
        if (!ShouldSendVectors(
                controlledId, _lastSentDirAt,
                (dir, _lastSentDir)))
        {
            return;
        }

        Rpc<ControlDirectionUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new ControlDirectionPacket(controlledId, fromVictim, dir));
    }

    private static void SendControlledStateIfNeeded(byte controlledId, Vector2 position, bool inAnim, Vector2 velocity)
    {
        if (!ShouldSendVectors(
                controlledId, _lastSentStateAt,
                (position, _lastSentPos),
                (velocity, _lastSentVel)))
        {
            return;
        }

        Rpc<ControlStateUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new ControlStatePacket(controlledId, position, inAnim, velocity));
    }

    public static bool ShouldSendVectors(
        byte targetId, Dictionary<byte, float> sentAt, params (Vector2, Dictionary<byte, Vector2>)[] vectors)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return false;
        }

        var now = Time.time;
        bool shouldSend = false;

        if (sentAt.TryGetValue(targetId, out var lastAt))
        {
            shouldSend |= (now - lastAt) >= MovementKeepAliveSeconds;
        }
        else
        {
            return false;
        }

        foreach (var (vector, lastSent) in vectors)
        {
            if (lastSent.TryGetValue(targetId, out var lastVector))
            {
                shouldSend |= (vector - lastVector).sqrMagnitude > MovementChangeEpsilonSqr;
            }
            else
            {
                return false;
            }
        }

        if (!shouldSend)
        {
            return false;
        }

        foreach (var (vector, lastSent) in vectors)
        {
            lastSent[targetId] = vector;
        }

        return true;
    }

    public static void GetControlledMovementState(this PlayerControl player, out Vector2 position, out bool inAnim, out Vector2 velocity)
    {
        inAnim = player.IsInAnimState();

        velocity = inAnim && player.MyPhysics?.body != null
            ? player.MyPhysics.body.velocity
            : Vector2.zero;
        position = player.MyPhysics?.body.position ?? player.transform.position;
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static bool PlayerPhysicsFixedUpdatePrefix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (player == null || player.Data == null)
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }
        
        if (!ControlState.IsUsingAnyState(player.PlayerId, out var otherId, out var isControlled, out var controlState))
        {
            return true;
        }

        var applyToController = controlState is DualControlState { ApplyToController: true } && !isControlled;

        if (player.AmOwner &&
            PlayerControl.LocalPlayer &&
            PlayerControl.LocalPlayer.TryGetRemoteControl<IRemoteControlActive>(out var control) &&
            control.Controlled is { } victim &&
            (control is IController || control.State is DualControlState))
        {
            var couldMoveIndependently = !applyToController && control.ControllerCouldMove;
            var canMoveIndependently = couldMoveIndependently && control.ControllerCanMove;

            Vector2 dir = canMoveIndependently ? GetSecondaryDirection() : GetNormalDirection();

            if (control.State.IsActionable(PlayerControl.LocalPlayer.PlayerId))
            {
                if (victim.Data == null || victim.HasDied() || victim.Data.Disconnected)
                {
                    return true;
                }

                if (control.State is DualControlState)
                {
                    var target = victim.AmOwner ? control.Controller : victim;
                    SendControlledDirectionIfNeeded(target.PlayerId, victim.AmOwner, dir);
                }
            }

            if (control is IController)
            {
                victim.GetControlledMovementState(out var victimPos, out var victimInAnim, out var victimVel);

                if (control.State is DualControlState)
                {
                    SendControlledStateIfNeeded(victim.PlayerId, victimPos, victimInAnim, victimVel);
                }
                else
                {
                    SendControlledInputIfNeeded(victim.PlayerId, dir, victimPos, victimInAnim, victimVel);
                }

                if (!applyToController)
                {
                    if (couldMoveIndependently && (Minigame.Instance || player.IsInAnimState()))
                    {
                        return true;
                    }

                    var controllerDir = canMoveIndependently ? GetPrimaryDirection() : Vector2.zero;

                    AdvancedMovementUtilities.ApplyControlledMovement(__instance, controllerDir);
                    return false;
                }
            }
        }

        if (applyToController)
        {
            ApplyAllDataTo(__instance, otherId, controlState);
            return false;
        }

        if (isControlled)
        {
            if (player.IsInAnimState())
            {
                return true;
            }

            ApplyAllDataTo(__instance, player.PlayerId, controlState);
            return false;
        }

        if (player.HasModifierOfType<IControlledModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (player.IsInAnimState())
            {
                return true;
            }

            ApplyAllDataTo(__instance, player.PlayerId, controlState);
            return false;
        }

        return true;
    }

    public static void ApplyAllDataTo(PlayerPhysics __instance, byte controlledId, ControlState controlState)
    {
        Vector2 dir = controlState.GetDirection(controlledId);
        Vector2 pos = controlState.GetPosition(controlledId);
        Vector2 vel = controlState.GetVelocity(controlledId);
        bool inAnim = controlState.IsInAnim(controlledId);

        __instance.HandleAnimation(__instance.myPlayer.Data.IsDead);

        if (__instance.body != null)
        {
            if (inAnim)
            {
                __instance.body.velocity = vel;
            }
            else
            {
                __instance.SetNormalizedVelocity(dir);
                // __instance.body.velocity = dir * __instance.TrueSpeed;
            }
        }

        if (pos != Vector2.zero)
        {
            var currentPos = __instance.body != null ? __instance.body.position : (Vector2)__instance.myPlayer.transform.position;
            var delta = pos - currentPos;
            if (delta.magnitude > 0.5f)
            {
                __instance.myPlayer.transform.position = pos;
                __instance.body?.position = pos;
            }
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    [HarmonyPrefix]
    public static bool CustomNetworkTransformFixedUpdatePrefix(CustomNetworkTransform __instance)
    {
        if (__instance.isPaused || !__instance.myPlayer)
        {
            return true;
        }

        var player = __instance.myPlayer;

        if (!ControlState.IsUsingAnyState(player.PlayerId, out _, out var isControlled, out var state))
        {
            return true;
        }

        if (state is not DualControlState && !isControlled)
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (player.IsInAnimState())
        {
            return true;
        }

        return false;
    }
}
