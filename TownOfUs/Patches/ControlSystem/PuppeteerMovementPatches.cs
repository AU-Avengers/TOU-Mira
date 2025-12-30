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

    private const float DirectionChangeEpsilonSqr = 0.0004f * 0.0004f;
    private const float DirectionKeepAliveSeconds = 0.6f;
    private static readonly Dictionary<byte, Vector2> _lastSentDir = new();
    private static readonly Dictionary<byte, float> _lastSentAt = new();
    private static readonly Dictionary<byte, Vector2> _localDesiredDir = new();

    private static void SendControlledInputIfNeeded(byte controlledId, Vector2 dir)
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        var now = Time.time;
        var shouldSend = true;

        if (_lastSentDir.TryGetValue(controlledId, out var lastDir) &&
            _lastSentAt.TryGetValue(controlledId, out var lastAt))
        {
            var changed = (dir - lastDir).sqrMagnitude > DirectionChangeEpsilonSqr;
            var keepAliveDue = dir != Vector2.zero && (now - lastAt) >= DirectionKeepAliveSeconds;
            shouldSend = changed || keepAliveDue;
        }

        if (!shouldSend)
        {
            return;
        }

        _lastSentDir[controlledId] = dir;
        _lastSentAt[controlledId] = now;

        Rpc<PuppeteerInputUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new PuppeteerInputPacket(controlledId, dir));
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

            var dir = GetNormalDirection();
            _localDesiredDir[puppeteer.Controlled.PlayerId] = dir;
            SendControlledInputIfNeeded(puppeteer.Controlled.PlayerId, dir);

            var puppeteerDir = Vector2.zero;
            
            AdvancedMovementUtilities.ApplyControlledMovement(__instance, puppeteerDir, stopIfZero: true);
            return false;
        }

        if (PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data?.Role is PuppeteerRole pr &&
            pr.Controlled != null &&
            player == pr.Controlled &&
            !player.AmOwner)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var dir = _localDesiredDir.TryGetValue(player.PlayerId, out var cached) ? cached : Vector2.zero;
            AdvancedMovementUtilities.ApplyControlledMovement(__instance, dir);

            return false;
        }

        if (player.HasModifier<PuppeteerControlModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var victimDir = PuppeteerControlState.GetDirection(player.PlayerId);
            AdvancedMovementUtilities.ApplyControlledMovement(__instance, victimDir);
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
        if (!PuppeteerControlState.IsControlled(player.PlayerId, out var controllerId))
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data?.Role is PuppeteerRole pr &&
            pr.Controlled != null &&
            player == pr.Controlled &&
            PlayerControl.LocalPlayer.PlayerId == controllerId)
        {
            return false;
        }

        return true;
    }
}
