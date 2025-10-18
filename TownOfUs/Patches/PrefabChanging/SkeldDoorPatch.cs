using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using PowerTools;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Maps;
using UnityEngine.ProBuilder;

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

        var newDoorList = new Il2CppReferenceArray<OpenableDoor>(doors.Length);
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
            plainDoor.SetDoorway(autoDoor.Open);
            consoleDoor.MinigamePrefab = polusdoor;
            autoDoor.Destroy();
            newDoorList.Add(plainDoor);
        }

        __instance.AllDoors = newDoorList;
        
        var newDict = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>();
        newDict.Add(SystemTypes.Electrical, new SwitchSystem().TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.MedBay, new MedScanSystem().TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.Doors, new DoorsSystemType().TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.Comms, new HudOverrideSystemType().TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.Security, new SecurityCameraSystemType().TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.Reactor, new ReactorSystemType(30f, SystemTypes.Reactor).TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(30f).TryCast<ISystemType>()!);
        newDict.Add(SystemTypes.Ventilation, new VentilationSystem().TryCast<ISystemType>()!);

        if (__instance.Systems.TryGetValue(SystemTypes.Sabotage, out var sabotage))
        {
            newDict.Add(SystemTypes.Sabotage, sabotage);
        }
        
        __instance.Systems = newDict;
    }
}