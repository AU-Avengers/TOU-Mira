using System.Reflection;
using HarmonyLib;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.MedSpirit;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class AmongUsClientPatches
{
    private static bool AlreadyApplied = false;
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void StartPatch(AmongUsClient __instance)
    {
        if (AlreadyApplied) return;
        AlreadyApplied = true;
        // This allows the custom door types to update properly
        Warning("Added TOU Mira System Types!");
        var systemTypesField = typeof(SystemTypeHelpers).GetField("AllTypes", BindingFlags.Public | BindingFlags.Static);
        if (systemTypesField != null)
        {
            systemTypesField.SetValue(null, SystemTypeHelpers.AllTypes
                .Concat([HexBombSabotageSystem.SystemType, SkeldDoorsSystemType.SystemType, ManualDoorsSystemType.SystemType])
                .Distinct()
                .ToArray());
        }

        Warning("Added TOU Mira Spawnables.");
        var medSpirit = TouAssets.MediumSpirit.LoadAsset().GetComponent<MedSpiritObject>();
        medSpirit.SpawnId = (uint)__instance.SpawnableObjects.Length;
        __instance.SpawnableObjects =
            __instance.SpawnableObjects.AddRangeToArray([__instance.SpawnableObjects[0]]); // dummy value

        __instance.NonAddressableSpawnableObjects = __instance.NonAddressableSpawnableObjects.AddRangeToArray([medSpirit]);
    }
}