using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class SnarerMovementPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CanMovePostfix(PlayerControl __instance, ref bool __result)
    {
        if (PlayerControl.LocalPlayer == null || __instance == null)
        {
            return;
        }

        if (__instance != PlayerControl.LocalPlayer)
        {
            return;
        }

        if (__instance.HasModifier<SnaredOnVentModifier>() && !MeetingHud.Instance && !TimeLordRewindSystem.IsRewinding)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static bool PlayerPhysicsFixedUpdatePrefix(PlayerPhysics __instance)
    {
        if (__instance == null)
        {
            return true;
        }

        var player = __instance.myPlayer;
        if (player == null || !player.AmOwner)
        {
            return true;
        }

        if (TimeLordRewindSystem.IsRewinding || MeetingHud.Instance)
        {
            return true;
        }

        if (!player.TryGetModifier<SnaredOnVentModifier>(out var snared) || !snared.TimerActive)
        {
            return true;
        }

        AdvancedMovementUtilities.ApplyControlledMovement(__instance, Vector2.zero, stopIfZero: true);
        player.transform.position = snared.VentTopPos;
        player.NetTransform.SnapTo(snared.VentTopPos);

        return false;
    }
}