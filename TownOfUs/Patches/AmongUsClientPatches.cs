using HarmonyLib;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.MedSpirit;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class AmongUsClientPatches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void StartPatch(AmongUsClient __instance)
    {
        if (AmongUsClient.Instance != __instance)
        {
            return;
        }
        // This allows the custom door types to update properly
        Warning("Added TOU Mira System Types!");
        SystemTypeHelpers.AllTypes.AddRangeToArray([HexBombSabotageSystem.SystemType, SkeldDoorsSystemType.SystemType, ManualDoorsSystemType.SystemType]);
        Error("TOU Mira Spawnables are temporarily disabled. Medium will not work.");

        // TODO: Fix Medium Spirit spawnable ASAP
        Warning("Added TOU Mira Spawnables.");
        var medSpirit = TouAssets.MediumSpirit.LoadAsset().GetComponent<MedSpiritObject>();
        medSpirit.SpawnId = (uint)__instance.SpawnableObjects.Length;
        __instance.SpawnableObjects =
            __instance.SpawnableObjects.AddRangeToArray([__instance.SpawnableObjects[0]]); // dummy value

        __instance.NonAddressableSpawnableObjects = __instance.NonAddressableSpawnableObjects.AddRangeToArray([medSpirit]);
    }
}