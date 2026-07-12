using System.Runtime.CompilerServices;
using HarmonyLib;

namespace TownOfUs.Patches.Cosmetics;

[HarmonyPatch]
public static class InventoryTabReversePatch
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(InventoryTab), nameof(InventoryTab.OnEnable))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony Reverse Patch")]
    public static void OnEnable(InventoryTab instance)
    {
        // stub
    }
}