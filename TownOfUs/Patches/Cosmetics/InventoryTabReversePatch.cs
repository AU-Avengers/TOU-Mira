using System.Runtime.CompilerServices;
using HarmonyLib;

namespace TownOfUs.Patches.Cosmetics;

[HarmonyPatch]
public static class InventoryTabReversePatch
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(InventoryTab), nameof(InventoryTab.OnEnable))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnEnable(InventoryTab instance)
    {
        // stub
    }
}