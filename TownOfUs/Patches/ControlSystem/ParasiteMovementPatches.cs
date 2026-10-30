using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using static TownOfUs.Patches.ControlSystem.ControlMovementPatches;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch]
public static class ParasiteMovementPatches
{
    private static readonly Dictionary<byte, Vector2> _lastSentDir = [];
    private static readonly Dictionary<byte, Vector2> _lastSentPos = [];
    private static readonly Dictionary<byte, Vector2> _lastSentVel = [];
    private static readonly Dictionary<byte, float> _lastSentAt = [];
    
    private static void SendControlledInputIfNeeded(byte controlledId, Vector2 dir, Vector2 position, bool inAnim, Vector2 velocity)
    {
        if (!ShouldSendVectors(
                controlledId, _lastSentAt,
                (dir,      _lastSentDir),
                (position, _lastSentPos),
                (velocity, _lastSentVel)))
        {
            return;
        }

        Rpc<FullControlInputUnreliableRpc>.Instance.Send(
            PlayerControl.LocalPlayer,
            new FullControlInputPacket(controlledId, dir, position, inAnim, velocity));
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

        if (player.AmOwner &&
            PlayerControl.LocalPlayer &&
            PlayerControl.LocalPlayer.Data?.Role is ParasiteRole { Controlled: { } victim })
        {
            if (victim.Data == null || victim.HasDied() || victim.Data.Disconnected)
            {
                return true;
            }

            var canMoveIndependently = OptionGroupSingleton<ParasiteOptions>.Instance.CanMoveIndependently;

            Vector2 dir = canMoveIndependently ? GetSecondaryDirection() : GetNormalDirection();

            victim.GetControlledMovementState(out var victimInAnim, out var victimVel, out var victimPos);

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

        return true;
    }
}