using HarmonyLib;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PassiveButton))]
public static class StabilityPatches
{
    [HarmonyPatch(nameof(PassiveButton.ReceiveClickDown))]
    [HarmonyPatch(nameof(PassiveButton.ReceiveClickUp))]
    [HarmonyPrefix]
    public static bool PrefixClick(PassiveButton __instance)
    {
        if (__instance == null || __instance.Pointer == IntPtr.Zero)
        {
            return false;
        }

        return true;
    }
}
