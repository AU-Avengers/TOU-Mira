using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;

namespace TownOfUs.Patches.PrefabChanging;

[HarmonyPatch]
public static class AirshipDoorPatch
{
    [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void Postfix(AirshipStatus __instance)
    {
        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.AirshipPolusDoors)
        {
            return;
        }

        var polusdoor = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
        {
            door.MinigamePrefab = polusdoor;
        }
    }
}