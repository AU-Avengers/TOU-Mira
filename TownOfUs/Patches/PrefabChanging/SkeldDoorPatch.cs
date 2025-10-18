using HarmonyLib;
using MiraAPI.GameOptions;
using PowerTools;
using TownOfUs.Options.Maps;

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

        // var skeldDoors = new Il2CppReferenceArray<OpenableDoor>(doors.Length);
        foreach (var door in doors)
        {
            var autoDoor = door.GetComponent<AutoOpenDoor>();
            var plainDoor = door.AddComponent<PlainDoor>();
            var consoleDoor = door.AddComponent<DoorConsole>();
            plainDoor.animator = door.GetComponent<SpriteAnim>();
            plainDoor.CloseDoorAnim = autoDoor.CloseDoorAnim;
            plainDoor.CloseSound = autoDoor.CloseSound;
            plainDoor.myCollider = autoDoor.myCollider;
            plainDoor.OpenDoorAnim = autoDoor.OpenDoorAnim;
            plainDoor.OpenSound = autoDoor.OpenSound;
            plainDoor.shadowCollider = autoDoor.shadowCollider;
            plainDoor.Id = autoDoor.Id;
            plainDoor.size = autoDoor.size;
            plainDoor.Room = autoDoor.Room;
            consoleDoor.MyDoor = autoDoor;
            plainDoor.SetDoorway(autoDoor.Open);
            plainDoor.Room = autoDoor.Room;
            consoleDoor.MinigamePrefab = polusdoor;
            // autoDoor.Destroy();
            // skeldDoors.Add(plainDoor);
        }

        __instance.Systems.Remove(SystemTypes.Doors);
        //__instance.AllDoors = _skeldDoors;
        __instance.Systems.Add(SystemTypes.Doors, new DoorsSystemType().TryCast<ISystemType>());
    }
}