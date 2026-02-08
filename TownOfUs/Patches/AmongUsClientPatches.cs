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
    public static void StartPatch(AmongUsClient __instance)
    {
        if (AmongUsClient.Instance != __instance)
        {
            Error("AmongUsClient duplicate detected.");
            return;
        }

        SystemTypeHelpers.AllTypes = SystemTypeHelpers.AllTypes.Concat([(SystemTypes)HexBombSabotageSystem.SabotageId, SkeldDoorsSystemType.SystemType, ManualDoorsSystemType.SystemType]).ToArray();

        var medSpirit = TouAssets.MediumSpirit.LoadAsset().GetComponent<MedSpiritObject>();
        medSpirit.SpawnId = (uint)__instance.SpawnableObjects.Count;
        __instance.SpawnableObjects =
            __instance.SpawnableObjects.Add(__instance.SpawnableObjects[0]).ToArray(); // dummy value

        __instance.NonAddressableSpawnableObjects =
            __instance.NonAddressableSpawnableObjects.Add(medSpirit).ToArray();
    }
}