using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;

namespace TownOfUs.Patches.PrefabChanging;

[HarmonyPatch]
public static class SkeldDoorPatch
{
    [HarmonyPatch(typeof(SkeldShipStatus), nameof(SkeldShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void Postfix(SkeldShipStatus __instance)
    {
        if (!OptionGroupSingleton<BetterSkeldOptions>.Instance.SkeldPolusDoors)
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