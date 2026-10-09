using HarmonyLib;
using UnityEngine;

namespace TownOfUs.DebugTools;

[HarmonyPatch]
internal static class FpsCounterPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static void HudManagerStartPostfix(HudManager __instance)
    {
        if (__instance == null || !TownOfUsPlugin.IsDevBuild)
        {
            return;
        }

        if (!__instance.gameObject.GetComponent<FpsCounter>())
        {
            __instance.gameObject.AddComponent<FpsCounter>();
        }
    }
}
