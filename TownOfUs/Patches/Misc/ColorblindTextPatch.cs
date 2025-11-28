using HarmonyLib;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.GetColorBlindText))]
public static class ColorblindTextPatch
{
    public static bool Prefix(CosmeticsLayer __instance, ref string __result)
    {
        var name = Palette.GetColorName(__instance.bodyMatProperties.ColorId).ToTitleCase();
        __result = name;

        return false;
    }
}
