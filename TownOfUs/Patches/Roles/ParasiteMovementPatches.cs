using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class ParasiteMovementPatches
{

    // I really don't know how to make Among Us Input system work properly here.. if someone knows how I'd love to know
    private static Vector2 GetWasdDirection() => TimeLordParasiteMovementUtilities.GetWasdDirection();

    private static Vector2 GetArrowDirection() => TimeLordParasiteMovementUtilities.GetArrowDirection();

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
            var parasiteDir = canMoveIndependently ? GetWasdDirection() : Vector2.zero;
            
            TimeLordParasiteMovementUtilities.ApplyParasiteMovement(__instance, parasiteDir, stopIfZero: true);
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
            var dir = canMoveIndependently ? GetArrowDirection() : GetWasdDirection();

            TimeLordParasiteMovementUtilities.ApplyParasiteMovement(__instance, dir);

            var vel = dir * __instance.TrueSpeed;
            var pos = __instance.body != null ? __instance.body.position : (Vector2)player.transform.position;
            Rpc<ParasiteMoveUnreliableRpc>.Instance.Send(PlayerControl.LocalPlayer,
                new ParasiteMovePacket(player.PlayerId, pos, vel));

            return false;
        }

        if (player.HasModifier<ParasiteInfectedModifier>() && player.GetComponent<DummyBehaviour>() != null)
        {
            if (TimeLordRewindSystem.IsRewinding)
            {
                return true;
            }

            var victimDir = ParasiteControlState.GetDirection(player.PlayerId);
            TimeLordParasiteMovementUtilities.ApplyParasiteMovement(__instance, victimDir);
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
            return false;
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