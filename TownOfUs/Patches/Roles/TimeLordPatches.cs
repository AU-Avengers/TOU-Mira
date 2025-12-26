using HarmonyLib;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class TimeLordPatches
{
    private static float _lastTaskSnapshotTime;
    private static float _taskSnapshotInterval = 0.02f;
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPostfix]
    public static void RecordSnapshotsPostfix(PlayerPhysics __instance)
    {
        if (__instance != null && __instance.myPlayer == PlayerControl.LocalPlayer)
        {
            TimeLordRewindSystem.RecordLocalSnapshot(__instance);
            _lastTaskSnapshotTime = Time.time;

            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                TimeLordRewindSystem.RecordHostBodyPositions();
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
        {
            return;
        }

        var inMinigame = Minigame.Instance != null || SpawnInMinigame.Instance != null;
        if (!inMinigame)
        {
            return;
        }

        var now = Time.time;
        if (now - _lastTaskSnapshotTime < _taskSnapshotInterval)
        {
            return;
        }

        var physics = PlayerControl.LocalPlayer.MyPhysics;
        if (physics != null)
        {
            TimeLordRewindSystem.RecordLocalSnapshot(physics);
            _lastTaskSnapshotTime = now;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool PlayerPhysicsFixedUpdatePrefix(PlayerPhysics __instance)
    {
        if (__instance == null || __instance.myPlayer == null)
        {
            return true;
        }

        var player = __instance.myPlayer;
        
        // Handle rewind for local player (including infected players - they're already recorded in normal snapshots)
        if (PlayerControl.LocalPlayer != null && player == PlayerControl.LocalPlayer &&
            TimeLordRewindSystem.IsRewinding)
        {
            return !TimeLordRewindSystem.TryHandleRewindPhysics(__instance);
        }

        return true;
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FlipX), MethodType.Setter)]
    [HarmonyPrefix]
    public static void PlayerFlipXRewindPrefix(PlayerPhysics __instance, ref bool value)
    {
        if (TimeLordRewindSystem.IsRewinding &&
    __instance != null &&
    __instance.myPlayer != null &&
    __instance.myPlayer == PlayerControl.LocalPlayer)
        {
            value = !value;
        }
    }
}