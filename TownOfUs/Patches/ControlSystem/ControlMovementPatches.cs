using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
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

    public static void GetControlledMovementState(this PlayerControl player, out bool inAnim, out Vector2 velocity, out Vector2 position)
    {
        inAnim = player.IsInTargetingAnimState() || player.inVent;

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
        
        if (!ControlState.IsUsingAnyState(player.PlayerId, out _, out var isControlled, out var controlState))
        {
            return true;
        }

        if (isControlled)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            if (player.IsInTargetingAnimState() || player.inVent)
            {
                return true;
            }

            ApplyAllDataTo(__instance, player.PlayerId, controlState);
            return false;
        }

        if (player.HasModifierOfType<IControlledModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            if (player.IsInTargetingAnimState() || player.inVent)
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

        if (!ControlState.IsUsingAnyState(player.PlayerId, out _, out _, out _))
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (player.IsInTargetingAnimState() || player.inVent)
        {
            return true;
        }

        return false;
    }
}
