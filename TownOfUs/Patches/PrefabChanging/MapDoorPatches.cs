using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
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

    public static MapDoorType ActiveDoorType
    {
        get
        {
            var currentDoorType = MiscUtils.GetCurrentMap switch
            {
                ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => (MapDoorType)OptionGroupSingleton<BetterSkeldOptions>.Instance.SkeldDoorType.Value,
                ExpandedMapNames.Polus => (MapDoorType)OptionGroupSingleton<BetterPolusOptions>.Instance.PolusDoorType.Value,
                ExpandedMapNames.Airship => (MapDoorType)OptionGroupSingleton<BetterAirshipOptions>.Instance.AirshipDoorType.Value,
                ExpandedMapNames.Fungle => (MapDoorType)OptionGroupSingleton<BetterFungleOptions>.Instance.FungleDoorType.Value,
                ExpandedMapNames.Submerged => (MapDoorType)OptionGroupSingleton<BetterSubmergedOptions>.Instance.SubmergedDoorType.Value,
                _ => MapDoorType.None
            };
            return currentDoorType is MapDoorType.Random ? RandomDoorType : currentDoorType;
        }
    }
    
    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.CanUseDoors), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool CanUseDoors(InfectedOverlay __instance, ref bool __result)
    {
        __result = ShipStatus.Instance.Type == ShipStatus.MapType.Pb && ActiveDoorType is not MapDoorType.Skeld || !__instance.sabSystem.AnyActive;
        return false;
    }
    
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcCloseDoorsOfType))]
    [HarmonyPrefix]
    public static bool RpcCloseDoorsOfType(ShipStatus __instance, SystemTypes type)
    {
        if (type is not SystemTypes.Doors || !__instance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType))
        {
            return true;
        }
        
        if (AmongUsClient.Instance.AmHost)
        {
            __instance.CloseDoorsOfType(type);
            return false;
        }
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 27, (SendOption)1, AmongUsClient.Instance.HostId);
        messageWriter.Write((byte)type);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        return false;
    }
    
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(byte))]
    [HarmonyPrefix]
    public static bool RpcUpdateSystem(ShipStatus __instance, SystemTypes systemType, byte amount)
    {
        if (systemType is not SystemTypes.Doors || !__instance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType) || !__instance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            return true;
        }
        var newSysType = SystemTypes.Doors;
        if (__instance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType))
        {
            newSysType = SkeldDoorsSystemType.SystemType;
        }
        else if (__instance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            newSysType = ManualDoorsSystemType.SystemType;
        }
        if (AmongUsClient.Instance.AmHost)
        {
            __instance.UpdateSystem(newSysType, PlayerControl.LocalPlayer, amount);
            return false;
        }
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 35, (SendOption)1, AmongUsClient.Instance.HostId);
        messageWriter.Write((byte)newSysType);
        messageWriter.WriteNetObject(PlayerControl.LocalPlayer);
        messageWriter.Write(amount);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        return false;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(MessageWriter))]
    [HarmonyPrefix]
    public static bool RpcUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageWriter msgWriter)
    {
        if (systemType is not SystemTypes.Doors || !__instance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType) || !__instance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            return true;
        }
        var newSysType = SystemTypes.Doors;
        if (__instance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType))
        {
            newSysType = SkeldDoorsSystemType.SystemType;
        }
        else if (__instance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            newSysType = ManualDoorsSystemType.SystemType;
        }
        if (AmongUsClient.Instance.AmHost)
        {
            MessageReader msgReader = MessageReader.Get(msgWriter.ToByteArray(false));
            __instance.UpdateSystem(newSysType, PlayerControl.LocalPlayer, msgReader);
            return false;
        }
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 35, (SendOption)1, AmongUsClient.Instance.HostId);
        messageWriter.Write((byte)newSysType);
        messageWriter.WriteNetObject(PlayerControl.LocalPlayer);
        messageWriter.Write(msgWriter, false);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        return false;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    [HarmonyPrefix]
    public static bool CloseDoorsOfTypePrefix(SystemTypes room)
    {
        var instance = ShipStatus.Instance;
        if (instance.Systems.TryGetValue(SkeldDoorsSystemType.SystemType, out var systemType))
        {
            Info(string.Format(TownOfUsPlugin.Culture, "Closing doors of room {0}", room));
            var doorSys = systemType.Cast<IDoorSystem>();
            doorSys.CloseDoorsOfType(room);
            return false;
        }

        if (instance.Systems.TryGetValue(ManualDoorsSystemType.SystemType, out var systemType2))
        {
            Info(string.Format(TownOfUsPlugin.Culture, "Closing doors of room {0}", room));
            var doorSys = systemType2.Cast<IDoorSystem>();
            doorSys.CloseDoorsOfType(room);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    [HarmonyPostfix]
    public static void OverlayShowPatch(MapBehaviour __instance)
    {
        if (!ShipStatus.Instance.Systems.ContainsKey(SystemTypes.Doors) &&
            !ShipStatus.Instance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType) &&
            !ShipStatus.Instance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
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

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.DoorsUpdate))]
    [HarmonyPrefix]
    public static bool DoorsUpdatePrefix(MapRoom __instance)
    {
        var instance = ShipStatus.Instance;
        if (__instance.door && instance && instance.Systems.TryGetValue(SkeldDoorsSystemType.SystemType, out var systemType))
        {
            float timer = systemType.Cast<RunTimer>().GetTimer(__instance.room);
            float num = __instance.Parent.CanUseDoors ? timer : 1f;
            __instance.door.material.SetFloat("_Percent", num);
            return false;
        }

        if (__instance.door && instance && instance.Systems.TryGetValue(ManualDoorsSystemType.SystemType, out var systemType2))
        {
            float timer = systemType2.Cast<RunTimer>().GetTimer(__instance.room);
            float num = __instance.Parent.CanUseDoors ? timer : 1f;
            __instance.door.material.SetFloat("_Percent", num);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageDoors))]
    [HarmonyPrefix]
    public static bool SabotageDoorsPrefix(MapRoom __instance)
    {
        if (!__instance.Parent.CanUseDoors)
        {
            return false;
        }

        var instance = ShipStatus.Instance;
        if (instance.Systems.TryGetValue(SkeldDoorsSystemType.SystemType, out var systemType))
        {
            if (systemType.Cast<RunTimer>().GetTimer(__instance.room) > 0f)
            {
                return false;
            }
            instance.RpcCloseDoorsOfType(__instance.room);
            DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SkeldDoorsSystemType.SystemType);
            return false;
        }

        if (instance.Systems.TryGetValue(ManualDoorsSystemType.SystemType, out var systemType2))
        {
            if (systemType2.Cast<RunTimer>().GetTimer(__instance.room) > 0f)
            {
                return false;
            }
            instance.RpcCloseDoorsOfType(__instance.room);
            DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(ManualDoorsSystemType.SystemType);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(InfectedOverlay __instance)
    {
        if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out var systemType))
        {
            __instance.doors = systemType.Cast<IActivatable>();
        }
        else if (ShipStatus.Instance.Systems.TryGetValue(SkeldDoorsSystemType.SystemType, out var systemType2))
        {
            __instance.doors = systemType2.Cast<IActivatable>();
        }
        else if (ShipStatus.Instance.Systems.TryGetValue(ManualDoorsSystemType.SystemType, out var systemType3))
        {
            __instance.doors = systemType3.Cast<IActivatable>();
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

        if (doorType is MapDoorType.Skeld || doorType is MapDoorType.Submerged/* && !ModCompatibility.SubLoaded*/)
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
                doorMinigame = PrefabLoader.Submerged!.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
        }
        var doorList = new List<OpenableDoor>();

        foreach (var door in doors)
        {
            var autoDoor = door.GetComponent<AutoOpenDoor>();
            
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

            var plainDoor = door.AddComponent<PlainDoor>();
            var consoleDoor = door.AddComponent<DoorConsole>();

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
        __instance.Systems.Add(ManualDoorsSystemType.SystemType, new ManualDoorsSystemType().TryCast<ISystemType>());
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

        if (doorType is MapDoorType.Polus || doorType is MapDoorType.Submerged/* && !ModCompatibility.SubLoaded*/)
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
                __instance.Systems.Add(SkeldDoorsSystemType.SystemType, new SkeldDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = PrefabLoader.Submerged!.GetComponentInChildren<DoorConsole>().MinigamePrefab;
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

        if (doorType is MapDoorType.Airship || doorType is MapDoorType.Submerged/* && !ModCompatibility.SubLoaded*/)
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
                __instance.Systems.Add(SkeldDoorsSystemType.SystemType, new SkeldDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Fungle:
                doorMinigame = PrefabLoader.Fungle.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = PrefabLoader.Submerged!.GetComponentInChildren<DoorConsole>().MinigamePrefab;
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

        if (doorType is MapDoorType.Fungle || doorType is MapDoorType.Submerged/* && !ModCompatibility.SubLoaded*/)
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
                __instance.Systems.Add(SkeldDoorsSystemType.SystemType, new SkeldDoorsSystemType().TryCast<ISystemType>());

                return;
            case MapDoorType.Airship:
                doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
                break;
            case MapDoorType.Submerged:
                doorMinigame = PrefabLoader.Submerged!.GetComponentInChildren<DoorConsole>().MinigamePrefab;
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

        if (doorType is MapDoorType.Submerged || doorType is MapDoorType.Skeld || doorType is MapDoorType.None)
        {
            return;
        }

        var doorMinigame = PrefabLoader.Airship.GetComponentInChildren<DoorConsole>().MinigamePrefab;
        // Ignores elevator doors
        var doors = __instance.GetComponentsInChildren<PlainDoor>()
            .Where(x => !x.gameObject.name.Contains("Inner") && !x.gameObject.name.Contains("Outer"))
            .Select(x => x.gameObject).ToArray();
        var doorList = __instance.AllDoors.ToList();
        switch (doorType)
        {
            // TODO: Add compatibility with the removal of doors if possible, as the game will prevent players from moving because of this Submerged code: https://github.com/SubmergedAmongUs/Submerged/blob/38686ed9d5cf9bf7e90219c05996366ca38b565a/Submerged/SpawnIn/SubmarineSelectSpawn.cs#L432
            case MapDoorType.None:
                /*doors.DoIf(x => !x.gameObject.name.Contains("Inner") && !x.gameObject.name.Contains("Outer"), x => x.gameObject.Destroy());

                __instance.AllDoors = new Il2CppReferenceArray<OpenableDoor>(doors.Length);
                __instance.Systems.Remove(SystemTypes.Doors);*/
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
                __instance.Systems.Add(SkeldDoorsSystemType.SystemType, new SkeldDoorsSystemType().TryCast<ISystemType>());

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