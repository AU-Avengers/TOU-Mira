using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch]
public static class PuppeteerMovementPatches
{
    private static Vector2 GetNormalDirection() => AdvancedMovementUtilities.GetRegularDirection();

    private const float MovementChangeEpsilonSqr = 0.0001f * 0.0001f;
    private const float MovementKeepAliveSeconds = 0.03f;
    private static readonly Dictionary<byte, Vector2> _lastSentDir = new();
    private static readonly Dictionary<byte, Vector2> _lastSentPos = new();
    private static readonly Dictionary<byte, Vector2> _lastSentVel = new();
    private static readonly Dictionary<byte, float> _lastSentAt = new();
    private static readonly Dictionary<byte, Vector2> _localDesiredDir = new();

    private static void SendControlledInputIfNeeded(byte controlledId, Vector2 dir, Vector2 position, Vector2 velocity)
    {
        if (PlayerControl.LocalPlayer == null)
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

        Rpc<PuppeteerInputUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new PuppeteerInputPacket(controlledId, dir, position, velocity));
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
            if (player.HasModifier<PuppeteerControlModifier>() && player.AmOwner)
            {
                return true;
            }
            if (player == PlayerControl.LocalPlayer)
            {
                return true;
            }
        }


        if (player == PlayerControl.LocalPlayer &&
            PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data?.Role is PuppeteerRole puppeteer &&
            puppeteer.Controlled != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var victim = puppeteer.Controlled;
            
            if (victim == null || victim.Data == null || victim.HasDied() || victim.Data.Disconnected)
            {
                return true;
            }

            var victimId = victim.PlayerId;
            var victimInAnim = victim.IsInTargetingAnimState() ||
                               victim.inVent ||
                               victim.inMovingPlat ||
                               victim.onLadder ||
                               victim.walkingToVent;

            var dir = victimInAnim ? Vector2.zero : GetNormalDirection();
            _localDesiredDir[victimId] = dir;

            var victimPos = victim.MyPhysics.body != null 
                ? victim.MyPhysics.body.position 
                : (Vector2)victim.transform.position;
            var victimVel = victim.MyPhysics.body != null 
                ? victim.MyPhysics.body.velocity 
                : Vector2.zero;

            SendControlledInputIfNeeded(victimId, dir, victimPos, victimVel);

            var puppeteerDir = Vector2.zero;
            
            AdvancedMovementUtilities.ApplyControlledMovement(__instance, puppeteerDir, stopIfZero: true);
            return false;
        }

        if (PuppeteerControlState.IsControlled(player.PlayerId, out _))
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            if (player.onLadder || player.inMovingPlat)
            {
                PuppeteerControlState.ClearMovementState(player.PlayerId);
                return true;
            }

            if (player.IsInTargetingAnimState() || player.inVent || player.walkingToVent)
            {
                return true;
            }

            var dir = PuppeteerControlState.GetDirection(player.PlayerId);
            
            if (player.AmOwner)
            {
                AdvancedMovementUtilities.ApplyControlledMovement(__instance, dir, stopIfZero: true);
                return false;
            }
            
            var pos = PuppeteerControlState.GetPosition(player.PlayerId);
            var vel = PuppeteerControlState.GetVelocity(player.PlayerId);
            
            if (dir == Vector2.zero)
            {
                AdvancedMovementUtilities.ApplyControlledMovement(__instance, Vector2.zero, stopIfZero: true);
            }
            else if (pos != Vector2.zero || vel != Vector2.zero || dir != Vector2.zero)
            {
                AdvancedMovementUtilities.ApplyInjectedMovement(__instance, pos, vel, dir);
            }
            else
            {
                var cachedDir = _localDesiredDir.TryGetValue(player.PlayerId, out var cached) ? cached : Vector2.zero;
                AdvancedMovementUtilities.ApplyControlledMovement(__instance, cachedDir);
            }

            return false;
        }

        if (player.HasModifier<PuppeteerControlModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            if (player.onLadder || player.inMovingPlat)
            {
                PuppeteerControlState.ClearMovementState(player.PlayerId);
                return true;
            }

            if (player.IsInTargetingAnimState() || player.inVent || player.walkingToVent)
            {
                return true;
            }

            var dir = PuppeteerControlState.GetDirection(player.PlayerId);
            var pos = PuppeteerControlState.GetPosition(player.PlayerId);
            var vel = PuppeteerControlState.GetVelocity(player.PlayerId);
            
            if (pos != Vector2.zero || vel != Vector2.zero || dir != Vector2.zero)
            {
                AdvancedMovementUtilities.ApplyInjectedMovement(__instance, pos, vel, dir);
            }
            else
            {
                AdvancedMovementUtilities.ApplyControlledMovement(__instance, Vector2.zero, stopIfZero: true);
            }
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetNormalizedVelocity))]
    [HarmonyPrefix]
    public static bool SetNormalizedVelocityPrefix(PlayerPhysics __instance, ref Vector2 direction)
    {
        var player = __instance.myPlayer;
        if (player == null || !player.HasModifier<PuppeteerControlModifier>())
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (player.AmOwner && PuppeteerControlState.IsControlled(player.PlayerId, out _))
        {
            direction = PuppeteerControlState.GetDirection(player.PlayerId);
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
        if (!PuppeteerControlState.IsControlled(player.PlayerId, out _))
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