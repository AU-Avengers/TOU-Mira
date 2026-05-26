using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public static class ModStampPatch
{
    public static ModStampLocation StampPlacement = ModStampLocation.TopLeft;
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

        if (__instance.localCamera)
        {
            var position = StampPlacement switch
            {
                ModStampLocation.TopLeft => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftTop,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f)),
                ModStampLocation.BottomLeft => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftBottom,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f)),
                ModStampLocation.BottomRight => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.RightBottom,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f)),
                _ => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.RightTop,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f))
            };
            __instance.ModStamp.transform.position = position;
        }
        return false;
    }
}