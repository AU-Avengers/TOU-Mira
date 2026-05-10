using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public static class ModStampPatch
{
    [HarmonyPrefix]
    public static bool LateUpdate(ModManager __instance)
    {
        if (!__instance.localCamera)
        {
            if (HudManager.InstanceExists)
            {
                __instance.localCamera = HudManager.Instance.GetComponentInChildren<Camera>();
            }
            else
            {
                __instance.localCamera = Camera.main;
            }
        }
        // __instance.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.RightTop, new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f));
        __instance.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftTop, new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f));
        return false;
    }
}