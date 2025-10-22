using HarmonyLib;
using MiraAPI.GameOptions;
using PowerTools;
using TownOfUs.Modules;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Maps;

namespace TownOfUs.Patches.PrefabChanging;

[HarmonyPatch]
public static class MapDoorPatches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void SkeldDoorPatchPostfix(ShipStatus __instance)
    {
        var doorType = (MapDoorType)OptionGroupSingleton<BetterSkeldOptions>.Instance.SkeldDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            doorType = RandomDoorMapOptions.GetRandomDoorType(doorType);
        }
        if (doorType is MapDoorType.Skeld || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        switch (doorType)
        {
            case MapDoorType.Airship:
                doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
        }

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
            consoleDoor.MinigamePrefab = doorMinigame;
            // autoDoor.Destroy();
            // skeldDoors.Add(plainDoor);
        }

        __instance.Systems.Remove(SystemTypes.Doors);
        //__instance.AllDoors = _skeldDoors;
        __instance.Systems.Add(SystemTypes.Doors, new DoorsSystemType().TryCast<ISystemType>());
    }

    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void PolusDoorPatchPostfix(PolusShipStatus __instance)
    {
        var doorType = (MapDoorType)OptionGroupSingleton<BetterPolusOptions>.Instance.PolusDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            doorType = RandomDoorMapOptions.GetRandomDoorType(doorType);
        }
        if (doorType is MapDoorType.Polus || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        switch (doorType)
        {
            case MapDoorType.Skeld:
                var doors = __instance.GetComponentsInChildren<PlainDoor>().Select(x => x.gameObject).ToArray();
                var skeldDoors = __instance.AllDoors.ToList();
                foreach (var door in doors)
                {
                    var autoDoor = door.AddComponent<AutoOpenDoor>();
                    var plainDoor = door.GetComponent<PlainDoor>();
                    var consoleDoor = door.GetComponent<DoorConsole>();

                    var animator = door.GetComponent<SpriteAnim>();
                    var closeDoorAnim = plainDoor.CloseDoorAnim;
                    var closeSound = plainDoor.CloseSound;
                    var myCollider = plainDoor.myCollider;
                    var openDoorAnim = plainDoor.OpenDoorAnim;
                    var openSound = plainDoor.OpenSound;
                    var shadowCollider = plainDoor.shadowCollider;
                    var id = plainDoor.Id;
                    var size = plainDoor.size;
                    var room = plainDoor.Room;

                    skeldDoors.Remove(plainDoor);
                    plainDoor.Destroy();

                    autoDoor.animator = animator;
                    autoDoor.CloseDoorAnim = closeDoorAnim;
                    autoDoor.CloseSound = closeSound;
                    autoDoor.myCollider = myCollider;
                    autoDoor.OpenDoorAnim = openDoorAnim;
                    autoDoor.OpenSound = openSound;
                    autoDoor.shadowCollider = shadowCollider;
                    autoDoor.Id = id;
                    autoDoor.size = size;
                    autoDoor.Room = room;
                    autoDoor.SetDoorway(plainDoor.Open);
                    autoDoor.Room = plainDoor.Room;

                    skeldDoors.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = skeldDoors.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
        }

        foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
        {
            door.MinigamePrefab = doorMinigame;
        }
    }

    [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void AirshipDoorPatchPostfix(AirshipStatus __instance)
    {
        var doorType = (MapDoorType)OptionGroupSingleton<BetterAirshipOptions>.Instance.AirshipDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            doorType = RandomDoorMapOptions.GetRandomDoorType(doorType);
        }
        if (doorType is MapDoorType.Airship || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        switch (doorType)
        {
            case MapDoorType.Skeld:
                var doors = __instance.GetComponentsInChildren<PlainDoor>().Select(x => x.gameObject).ToArray();
                var skeldDoors = __instance.AllDoors.ToList();
                foreach (var door in doors)
                {
                    var autoDoor = door.AddComponent<AutoOpenDoor>();
                    var plainDoor = door.GetComponent<PlainDoor>();
                    var consoleDoor = door.GetComponent<DoorConsole>();

                    var animator = door.GetComponent<SpriteAnim>();
                    var closeDoorAnim = plainDoor.CloseDoorAnim;
                    var closeSound = plainDoor.CloseSound;
                    var myCollider = plainDoor.myCollider;
                    var openDoorAnim = plainDoor.OpenDoorAnim;
                    var openSound = plainDoor.OpenSound;
                    var shadowCollider = plainDoor.shadowCollider;
                    var id = plainDoor.Id;
                    var size = plainDoor.size;
                    var room = plainDoor.Room;

                    skeldDoors.Remove(plainDoor);
                    plainDoor.Destroy();

                    autoDoor.animator = animator;
                    autoDoor.CloseDoorAnim = closeDoorAnim;
                    autoDoor.CloseSound = closeSound;
                    autoDoor.myCollider = myCollider;
                    autoDoor.OpenDoorAnim = openDoorAnim;
                    autoDoor.OpenSound = openSound;
                    autoDoor.shadowCollider = shadowCollider;
                    autoDoor.Id = id;
                    autoDoor.size = size;
                    autoDoor.Room = room;
                    autoDoor.SetDoorway(plainDoor.Open);
                    autoDoor.Room = plainDoor.Room;

                    skeldDoors.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = skeldDoors.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
        }

        foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
        {
            door.MinigamePrefab = doorMinigame;
        }
    }

    [HarmonyPatch(typeof(FungleShipStatus), nameof(FungleShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void FungleDoorPatchPostfix(FungleShipStatus __instance)
    {
        var doorType = (MapDoorType)OptionGroupSingleton<BetterFungleOptions>.Instance.FungleDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            doorType = RandomDoorMapOptions.GetRandomDoorType(doorType);
        }
        if (doorType is MapDoorType.Fungle || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        switch (doorType)
        {
            // TODO: Fix Skeld doors on fungle, they error out entirely on update and will likely require a component replacement
            case MapDoorType.Skeld:
                var doors = __instance.GetComponentsInChildren<MushroomWallDoor>().Select(x => x.gameObject).ToArray();
                var skeldDoors = __instance.AllDoors.ToList();
                foreach (var door in doors)
                {
                    var plainDoor = door.GetComponent<MushroomWallDoor>();
                    var consoleDoor = door.GetComponent<DoorConsole>();

                    var closeSound = plainDoor.closeSound;
                    var openSound = plainDoor.openSound;
                    var wallCollider = plainDoor.wallCollider;
                    var shadowColl = plainDoor.shadowColl;
                    var bottomColl = plainDoor.bottomColl;
                    var mushrooms = plainDoor.mushrooms;
                    var id = plainDoor.Id;
                    var room = plainDoor.Room;

                    skeldDoors.Remove(plainDoor);
                    plainDoor.Destroy();

                    var autoDoor = door.AddComponent<AutoOpenMushroomDoor>();

                    autoDoor.closeSound = closeSound;
                    autoDoor.openSound = openSound;
                    autoDoor.wallCollider = wallCollider;
                    autoDoor.shadowColl = shadowColl;
                    autoDoor.bottomColl = bottomColl;
                    autoDoor.mushrooms = mushrooms;
                    autoDoor.Id = id;
                    autoDoor.Room = room;
                    autoDoor.SetDoorway(true);

                    skeldDoors.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = skeldDoors.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Airship:
                doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
        }

        foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
        {
            door.MinigamePrefab = doorMinigame;
        }
    }
}