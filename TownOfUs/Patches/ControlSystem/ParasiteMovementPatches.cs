using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch]
public static class ParasiteMovementPatches
{
    private static Vector2 GetPrimaryDirection() => AdvancedMovementUtilities.GetControllerPrimaryDirection();

    private static Vector2 GetSecondaryDirection() => AdvancedMovementUtilities.GetControllerSecondaryDirection();

    private static Vector2 GetNormalDirection() => AdvancedMovementUtilities.GetRegularDirection();

    private const float MovementChangeEpsilonSqr = 0.0001f * 0.0001f;
    private const float MovementKeepAliveSeconds = 0.03f;
    private static readonly Dictionary<byte, Vector2> _lastSentDir = [];
    private static readonly Dictionary<byte, Vector2> _lastSentPos = [];
    private static readonly Dictionary<byte, Vector2> _lastSentVel = [];
    private static readonly Dictionary<byte, float> _lastSentAt = [];
    
    private static void SendControlledInputIfNeeded(byte controlledId, Vector2 dir, Vector2 position, bool inAnim, Vector2 velocity)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var now = Time.time;
        var shouldSend = true;

        if (_lastSentDir.TryGetValue(controlledId, out var lastDir) &&
            _lastSentPos.TryGetValue(controlledId, out var lastPos) &&
            _lastSentVel.TryGetValue(controlledId, out var lastVel) &&
            _lastSentAt.TryGetValue(controlledId, out var lastAt))
        {
            var dirChanged = (dir - lastDir).sqrMagnitude > MovementChangeEpsilonSqr;
            var posChanged = (position - lastPos).sqrMagnitude > MovementChangeEpsilonSqr;
            var velChanged = (velocity - lastVel).sqrMagnitude > MovementChangeEpsilonSqr;
            var keepAliveDue = (now - lastAt) >= MovementKeepAliveSeconds;
            shouldSend = dirChanged || posChanged || velChanged || keepAliveDue;
        }

        if (!shouldSend)
        {
            return;
        }

        _lastSentDir[controlledId] = dir;
        _lastSentPos[controlledId] = position;
        _lastSentVel[controlledId] = velocity;
        _lastSentAt[controlledId] = now;

        Rpc<ParasiteInputUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new ParasiteInputPacket(controlledId, dir, position, inAnim, velocity));
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
            if (player.HasModifier<ParasiteInfectedModifier>() && player.AmOwner)
            {
                return true;
            }
            if (player.AmOwner)
            {
                return true;
            }
        }


        if (player.AmOwner &&
            PlayerControl.LocalPlayer &&
            PlayerControl.LocalPlayer.Data?.Role is ParasiteRole parasite &&
            parasite.Controlled != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var victim = parasite.Controlled;
            
            if (victim.Data == null || victim.HasDied() || victim.Data.Disconnected)
            {
                return true;
            }

            var canMoveIndependently = OptionGroupSingleton<ParasiteOptions>.Instance.CanMoveIndependently;

            Vector2 dir = canMoveIndependently ? GetSecondaryDirection() : GetNormalDirection();

            var victimInAnim = victim.IsInTargetingAnimState() || victim.inVent;

            var victimVel = victimInAnim && victim.MyPhysics?.body != null
                ? victim.MyPhysics.body.velocity
                : Vector2.zero;
            var victimPos = victim.MyPhysics?.body.position ?? victim.transform.position;

            SendControlledInputIfNeeded(victim.PlayerId, dir, victimPos, victimInAnim, victimVel);

            // TODO: apply movement to controller
            if (Minigame.Instance || player.inVent || player.inMovingPlat || player.onLadder || player.walkingToVent)
            {
                return true;
            }

            var parasiteDir = canMoveIndependently ? GetPrimaryDirection() : Vector2.zero;

            AdvancedMovementUtilities.ApplyControlledMovement(__instance, parasiteDir, stopIfZero: true);
            return false;
        }

        if (ParasiteControlState.IsControlled(player.PlayerId, out _))
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            if (player.IsInTargetingAnimState() || player.inVent)
            {
                return true;
            }

            ApplyAllDataTo(__instance, player.PlayerId);
            return false;
        }

        if (player.HasModifier<ParasiteInfectedModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            if (player.IsInTargetingAnimState() || player.inVent)
            {
                return true;
            }

            ApplyAllDataTo(__instance, player.PlayerId);
            return false;
        }

        return true;
    }

    private static void ApplyAllDataTo(PlayerPhysics __instance, byte controlledId)
    {
        Vector2 dir = ParasiteControlState.GetDirection(controlledId);
        Vector2 pos = ParasiteControlState.GetPosition(controlledId);
        Vector2 vel = ParasiteControlState.GetVelocity(controlledId);
        bool inAnim = ParasiteControlState.IsInAnim(controlledId);

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

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetNormalizedVelocity))]
    [HarmonyPrefix]
    public static bool SetNormalizedVelocityPrefix(PlayerPhysics __instance, ref Vector2 direction)
    {
        var player = __instance.myPlayer;
        if (player == null || !player.HasModifier<ParasiteInfectedModifier>())
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (player.AmOwner && ParasiteControlState.IsControlled(player.PlayerId, out _))
        {
            direction = ParasiteControlState.GetDirection(player.PlayerId);
        }

        return true;
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
        if (!ParasiteControlState.IsControlled(player.PlayerId, out _))
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (player.IsInTargetingAnimState() || player.inVent || player.inMovingPlat || player.onLadder || player.walkingToVent)
        {
            return true;
        }

        return false;
    }
}