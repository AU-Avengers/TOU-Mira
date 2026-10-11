using HarmonyLib;

namespace TownOfUs.DebugTools;

[HarmonyPatch]
internal static class FpsCounterPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static void HudManagerStartPostfix(HudManager __instance)
    {
        if (__instance == null)
        {
            return;
        }

        FpsCounter.showFps = LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ShowFpsCounter.Value;

        if (!__instance.gameObject.GetComponent<FpsCounter>())
        {
            __instance.gameObject.AddComponent<FpsCounter>();
        }
    }
}
