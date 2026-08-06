using HarmonyLib;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.MedSpirit;
using UnityEngine.ProBuilder;

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
        var customSysTypes = new List<SystemTypes>()
        {
            HexBombSabotageSystem.SystemType,
            SkeldDoorsSystemType.SystemType,
            ManualDoorsSystemType.SystemType,
        };
        // This allows the custom door types to update properly
        Warning("Added TOU Mira System Types!");
        SystemTypeHelpers.AllTypes = SystemTypeHelpers.AllTypes.Concat(customSysTypes).ToArray();

        Warning("Added TOU Mira Spawnables.");
        var medSpirit = TouAssets.MediumSpirit.LoadAsset().GetComponent<MedSpiritObject>();
        medSpirit.SpawnId = (uint)__instance.SpawnableObjects.Count;
        __instance.SpawnableObjects =
            __instance.SpawnableObjects.Add(__instance.SpawnableObjects[0]).ToArray(); // dummy value

        __instance.NonAddressableSpawnableObjects =
            __instance.NonAddressableSpawnableObjects.Add(medSpirit).ToArray();
    }
}