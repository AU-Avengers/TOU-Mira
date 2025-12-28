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
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch]
public static class ParasiteMovementPatches
{
    private static Vector2 GetPrimaryDirection() => AdvancedMovementUtilities.GetControllerPrimaryDirection();

    private static Vector2 GetSecondaryDirection() => AdvancedMovementUtilities.GetControllerSecondaryDirection();

    private static Vector2 GetNormalDirection() => AdvancedMovementUtilities.GetRegularDirection();

    private const float DirectionChangeEpsilonSqr = 0.0004f * 0.0004f;
    private const float DirectionKeepAliveSeconds = 0.25f;
    private static readonly Dictionary<byte, Vector2> _lastSentDir = new();
    private static readonly Dictionary<byte, float> _lastSentAt = new();

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
            var keepAliveDue = (now - lastAt) >= DirectionKeepAliveSeconds;
            shouldSend = changed || keepAliveDue;
        }

        if (!shouldSend)
        {
            return;
        }

        _lastSentDir[controlledId] = dir;
        _lastSentAt[controlledId] = now;

        Rpc<ParasiteInputUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new ParasiteInputPacket(controlledId, dir));
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
            if (player == PlayerControl.LocalPlayer)
            {
                return true;
            }
        }


        if (player == PlayerControl.LocalPlayer &&
            PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data?.Role is ParasiteRole parasite &&
            parasite.Controlled != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var canMoveIndependently = OptionGroupSingleton<ParasiteOptions>.Instance.CanMoveIndependently;
            var parasiteDir = canMoveIndependently ? GetPrimaryDirection() : Vector2.zero;
            
            AdvancedMovementUtilities.ApplyControlledMovement(__instance, parasiteDir, stopIfZero: true);
            return false;
        }

        if (PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data?.Role is ParasiteRole pr &&
            pr.Controlled != null &&
            player == pr.Controlled &&
            !player.AmOwner)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var canMoveIndependently = OptionGroupSingleton<ParasiteOptions>.Instance.CanMoveIndependently;
            var dir = canMoveIndependently ? GetSecondaryDirection() : GetNormalDirection();

            AdvancedMovementUtilities.ApplyControlledMovement(__instance, dir);
            SendControlledInputIfNeeded(player.PlayerId, dir);

            return false;
        }

        if (player.HasModifier<ParasiteInfectedModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var victimDir = ParasiteControlState.GetDirection(player.PlayerId);
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

    private static System.Reflection.FieldInfo? _sendQueueField;
    private static bool _sendQueueFieldSearched;

    private static System.Reflection.FieldInfo? GetSendQueueField()
    {
        if (!_sendQueueFieldSearched)
        {
            _sendQueueFieldSearched = true;
            var type = typeof(CustomNetworkTransform);

            _sendQueueField = AccessTools.DeclaredField(type, "sendQueue");

            if (_sendQueueField == null)
            {
                var allFields = AccessTools.GetDeclaredFields(type);
                _sendQueueField = allFields.FirstOrDefault(f =>
                    f.FieldType == typeof(System.Collections.Generic.Queue<Vector2>) &&
                    (f.Name == "sendQueue" ||
                     (f.Name.ToLowerInvariant().Contains("send", StringComparison.InvariantCultureIgnoreCase) &&
                      f.Name.ToLowerInvariant().Contains("queue", StringComparison.InvariantCultureIgnoreCase))));
            }

            if (_sendQueueField == null)
            {
                _sendQueueField = AccessTools.Field(type, "sendQueue");
            }
        }
        return _sendQueueField;
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    [HarmonyPrefix]
    public static bool CustomNetworkTransformFixedUpdatePrefix(CustomNetworkTransform __instance)
    {
        if (__instance.isPaused || !__instance.myPlayer)
        {
            return false;
        }

        var player = __instance.myPlayer;
        if (!ParasiteControlState.IsControlled(player.PlayerId, out var controllerId))
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return true;
        }

        if (PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.Data?.Role is ParasiteRole pr &&
            pr.Controlled != null &&
            player == pr.Controlled &&
            PlayerControl.LocalPlayer.PlayerId == controllerId)
        {
            return false;
        }

        if (player.AmOwner)
        {
            var queueField = GetSendQueueField();
            if (queueField != null)
            {
                var queue = (System.Collections.Generic.Queue<Vector2>?)queueField.GetValue(__instance);
                if (queue != null)
                {
                    queue.Enqueue(player.GetTruePosition());
                    __instance.SetDirtyBit(2U);
                    return false;
                }
            }

            return true;
        }

        return true;
    }
}
