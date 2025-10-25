using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using PowerTools;
using TownOfUs.Modules;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Maps;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.PrefabChanging;

[HarmonyPatch]
public static class MapDoorPatches
{
    public static MapDoorType RandomDoorType = MapDoorType.None;

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowCountOverlay))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    [HarmonyPostfix]
    public static void OverlayShowPatch(MapBehaviour __instance)
    {
        if (!ShipStatus.Instance.Systems.ContainsKey(SystemTypes.Doors))
        {
            if (__instance.infectedOverlay.allButtons.Any(x => x.gameObject.name == "closeDoors"))
            {
                __instance.infectedOverlay.allButtons.DoIf(x => x.gameObject.name == "closeDoors", x => x.gameObject.Destroy());
            }

            if (__instance.infectedOverlay.allButtons.Any(x => x.gameObject.name == "Doors"))
            {
                __instance.infectedOverlay.allButtons.DoIf(x => x.gameObject.name == "Doors", x => x.gameObject.Destroy());
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void SkeldDoorPatchPostfix(ShipStatus __instance)
    {
        if (MiscUtils.GetCurrentMap != ExpandedMapNames.Skeld && MiscUtils.GetCurrentMap != ExpandedMapNames.Dleks)
        {
            return;
        }
        var doorType = (MapDoorType)OptionGroupSingleton<BetterSkeldOptions>.Instance.SkeldDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            if (TutorialManager.InstanceExists)
            {
                doorType = RandomDoorMapOptions.GetRandomDoorType(MapDoorType.Skeld);
            }
            else
            {
                doorType = RandomDoorType;
            }
        }

        if (doorType is MapDoorType.Skeld || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        var doors = __instance.GetComponentsInChildren<AutoOpenDoor>().Select(x => x.gameObject).ToArray();
        switch (doorType)
        {
            case MapDoorType.None:
                doors.Do(x => x.Destroy());

                __instance.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
                __instance.Systems.Remove(SystemTypes.Doors);
                return;
            case MapDoorType.Airship:
                doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = ModCompatibility.SubmergedDoorMinigame.GetComponent<Minigame>();
                break;
        }
        var doorList = new List<OpenableDoor>();

        foreach (var door in doors)
        {
            var autoDoor = door.GetComponent<AutoOpenDoor>();
            var plainDoor = door.AddComponent<PlainDoor>();
            var consoleDoor = door.AddComponent<DoorConsole>();
            
            var animator = door.GetComponent<SpriteAnim>();
            var closeDoorAnim = autoDoor.CloseDoorAnim;
            var closeSound = autoDoor.CloseSound;
            var myCollider = autoDoor.myCollider;
            var openDoorAnim = autoDoor.OpenDoorAnim;
            var openSound = autoDoor.OpenSound;
            var shadowCollider = autoDoor.shadowCollider;
            var id = autoDoor.Id;
            var size = autoDoor.size;
            var room = autoDoor.Room;

            autoDoor.Destroy();

            plainDoor.animator = animator;
            plainDoor.CloseDoorAnim = closeDoorAnim;
            plainDoor.CloseSound = closeSound;
            plainDoor.myCollider = myCollider;
            plainDoor.OpenDoorAnim = openDoorAnim;
            plainDoor.OpenSound = openSound;
            plainDoor.shadowCollider = shadowCollider;
            plainDoor.Id = id;
            plainDoor.size = size;
            plainDoor.Room = room;
            plainDoor.SetDoorway(plainDoor.Open);
            consoleDoor.MinigamePrefab = doorMinigame;
            consoleDoor.MyDoor = plainDoor;
            
            var vector = plainDoor.myCollider.size;
            plainDoor.size = ((vector.x > vector.y) ? vector.y : vector.x);
            plainDoor.Open = plainDoor.myCollider.isTrigger;
            plainDoor.animator.Play(plainDoor.Open ? plainDoor.OpenDoorAnim : plainDoor.CloseDoorAnim, 1000f);
            plainDoor.UpdateShadow();

            doorList.Add(plainDoor);
            autoDoor.Destroy();
        }

        __instance.AllDoors = doorList.ToArray();
        __instance.Systems.Remove(SystemTypes.Doors);
        __instance.Systems.Add(SystemTypes.Doors, new DoorsSystemType().TryCast<ISystemType>());
    }

    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void PolusDoorPatchPostfix(PolusShipStatus __instance)
    {
        var doorType = (MapDoorType)OptionGroupSingleton<BetterPolusOptions>.Instance.PolusDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            if (TutorialManager.InstanceExists)
            {
                doorType = RandomDoorMapOptions.GetRandomDoorType(MapDoorType.Polus);
            }
            else
            {
                doorType = RandomDoorType;
            }
        }

        if (doorType is MapDoorType.Polus || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        var doors = __instance.GetComponentsInChildren<PlainDoor>().Select(x => x.gameObject).ToArray();
        var doorList = __instance.AllDoors.ToList();
        switch (doorType)
        {
            case MapDoorType.None:
                doors.Do(x => x.Destroy());

                __instance.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
                __instance.Systems.Remove(SystemTypes.Doors);
                return;
            case MapDoorType.Skeld:
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

                    doorList.Remove(plainDoor);
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

                    doorList.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = doorList.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = ModCompatibility.SubmergedDoorMinigame.GetComponent<Minigame>();
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
            if (TutorialManager.InstanceExists)
            {
                doorType = RandomDoorMapOptions.GetRandomDoorType(MapDoorType.Airship);
            }
            else
            {
                doorType = RandomDoorType;
            }
        }

        if (doorType is MapDoorType.Airship || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        var doors = __instance.GetComponentsInChildren<PlainDoor>().Select(x => x.gameObject).ToArray();
        var doorList = __instance.AllDoors.ToList();
        switch (doorType)
        {
            case MapDoorType.None:
                doors.Do(x => x.Destroy());

                __instance.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
                __instance.Systems.Remove(SystemTypes.Doors);
                return;
            case MapDoorType.Skeld:
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

                    doorList.Remove(plainDoor);
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

                    doorList.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = doorList.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = ModCompatibility.SubmergedDoorMinigame.GetComponent<Minigame>();
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
            if (TutorialManager.InstanceExists)
            {
                doorType = RandomDoorMapOptions.GetRandomDoorType(MapDoorType.Fungle);
            }
            else
            {
                doorType = RandomDoorType;
            }
        }

        if (doorType is MapDoorType.Fungle || doorType is MapDoorType.Submerged && !ModCompatibility.SubLoaded)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        var doors = __instance.GetComponentsInChildren<MushroomWallDoor>().Select(x => x.gameObject).ToArray();
        var doorList = __instance.AllDoors.ToList();
        switch (doorType)
        {
            case MapDoorType.None:
                doors.Do(x => x.Destroy());

                __instance.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
                __instance.Systems.Remove(SystemTypes.Doors);
                return;
            case MapDoorType.Skeld:
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

                    doorList.Remove(plainDoor);
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

                    doorList.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = doorList.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Airship:
                doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = ModCompatibility.SubmergedDoorMinigame.GetComponent<Minigame>();
                break;
        }

        foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
        {
            door.MinigamePrefab = doorMinigame;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void SubmergedDoorPatchPostfix(ShipStatus __instance)
    {
        if (!ModCompatibility.SubLoaded || MiscUtils.GetCurrentMap != ExpandedMapNames.Submerged)
        {
            return;
        }

        var doorType = (MapDoorType)OptionGroupSingleton<BetterSubmergedOptions>.Instance.SubmergedDoorType.Value;
        if (doorType is MapDoorType.Random)
        {
            if (TutorialManager.InstanceExists)
            {
                doorType = RandomDoorMapOptions.GetRandomDoorType(MapDoorType.Submerged);
            }
            else
            {
                doorType = RandomDoorType;
            }
        }

        if (doorType is MapDoorType.Submerged)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        var doors = __instance.GetComponentsInChildren<PlainDoor>().Select(x => x.gameObject).ToArray();
        var doorList = __instance.AllDoors.ToList();
        switch (doorType)
        {
            case MapDoorType.None:
                doors.Do(x => x.Destroy());

                __instance.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
                __instance.Systems.Remove(SystemTypes.Doors);
                return;
            case MapDoorType.Skeld:
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

                    doorList.Remove(plainDoor);
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

                    doorList.Add(autoDoor);

                    consoleDoor.Destroy();
                }

                __instance.AllDoors = doorList.ToArray();
                __instance.Systems.Remove(SystemTypes.Doors);
                __instance.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Polus:
                doorMinigame = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
        }

        foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
        {
            door.MinigamePrefab = doorMinigame;
        }
    }
}