using AmongUs.Data;
using HarmonyLib;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
public static class FollowerCameraPatches
{
    public static bool Prefix(FollowerCamera __instance)
    {
        if (!SpectatorRole.FixedCam)
        {
            return true;
        }

        if (!__instance.Target || __instance.Locked)
        {
            return false;
        }

        var v = (Vector2)__instance.Target.transform.position + __instance.Offset;

        if (__instance.shakeAmount > 0f && DataManager.Settings.Gameplay.ScreenShake &&
            __instance.OverrideScreenShakeEnabled)
        {
            var num = Time.fixedTime * __instance.shakePeriod;
            var num2 = (Mathf.PerlinNoise(0.5f, num) * 2f) - 1f;
            var num3 = (Mathf.PerlinNoise(num, 0.5f) * 2f) - 1f;
            v.x += num2 * __instance.shakeAmount;
            v.y += num3 * __instance.shakeAmount;
        }

        __instance.transform.position = v;
        return false;
    }
}