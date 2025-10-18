using HarmonyLib;
using MiraAPI.GameOptions;
using PowerTools;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Maps;
using UnityEngine;

namespace TownOfUs.Patches.PrefabChanging;

[HarmonyPatch]
public static class SkeldDoorPatch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void Postfix(ShipStatus __instance)
    {
        if (!OptionGroupSingleton<BetterSkeldOptions>.Instance.SkeldPolusDoors)
        {
            return;
        }

        var polusdoor = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        var doors = __instance.GetComponentsInChildren<AutoOpenDoor>().Select(x => x.gameObject).ToArray();
        foreach (var door in doors)
        {
            var autoDoor = door.GetComponent<AutoOpenDoor>();
            var consoleDoor = door.AddComponent<DoorConsole>();
            var plainDoor = door.AddComponent<PlainDoor>();
            plainDoor.animator = door.GetComponent<SpriteAnim>();
            plainDoor.CloseDoorAnim = autoDoor.CloseDoorAnim;
            plainDoor.CloseSound = autoDoor.CloseSound;
            plainDoor.myCollider = autoDoor.myCollider;
            plainDoor.OpenDoorAnim = autoDoor.OpenDoorAnim;
            plainDoor.OpenSound = autoDoor.OpenSound;
            plainDoor.shadowCollider = autoDoor.shadowCollider;
            plainDoor.size = autoDoor.size;
            plainDoor.Open = autoDoor.Open;
            consoleDoor.Image = door.GetComponent<SpriteRenderer>();
            consoleDoor.MyDoor = plainDoor;
            consoleDoor.MinigamePrefab = polusdoor;
            autoDoor.Destroy();
        }
    }
}