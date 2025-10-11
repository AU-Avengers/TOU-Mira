using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Reactor.Utilities;
using TownOfUs.Modules.Components;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;
using Version = SemanticVersioning.Version;

namespace TownOfUs.Modules;

public static class ModCompatibility
{
    public const string SubmergedGuid = "Submerged";
    public const ShipStatus.MapType SubmergedMapType = (ShipStatus.MapType)6;

    public const string LevelImpostorGuid = "com.DigiWorm.LevelImposter";
    public const ShipStatus.MapType LevelImpostorMapType = (ShipStatus.MapType)7;

    private static MethodInfo rpcRequestChangeFloor;
    private static MethodInfo registerFloorOverride;
    private static MethodInfo getFloorHandler;

    private static PropertyInfo inTransition;
    private static PropertyInfo submarineOxygenSystemInstance;
    private static MethodInfo repairDamage;

    private static MethodInfo getInElevator;
    private static MethodInfo getMovementStageFromTime;
    private static FieldInfo getSubElevatorSystem;

    private static FieldInfo upperDeckIsTargetFloor;

    private static FieldInfo submergedInstance;
    private static FieldInfo submergedElevators;

    public static FieldInfo lastMapID;

    private static PropertyInfo currentMap;
    private static PropertyInfo elements;

    private static PropertyInfo liElementType;
    private static PropertyInfo liElementName;

    public static Type MapObjectData;

    public static Version SubVersion { get; private set; }
    public static bool SubLoaded { get; private set; }
    public static BasePlugin SubPlugin { get; private set; }
    public static Assembly SubAssembly { get; private set; }
    public static Type[] SubTypes { get; private set; }
    public static Dictionary<string, Type> SubInjectedTypes { get; private set; }

    public static TaskTypes RetrieveOxygenMask { get; private set; }

    public static bool LILoaded { get; private set; }
    private static BasePlugin LIPlugin { get; set; }
    private static Assembly LIAssembly { get; set; }
    private static Type[] LITypes { get; set; }
    public static bool IsWikiButtonOffset { get; set; }

    public static void Initialize()
    {
        InitSubmerged();
        InitLevelImpostor();
    }

    public static void InitSubmerged()
    {
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(SubmergedGuid, out var plugin))
        {
            return;
        }

        SubPlugin = (plugin!.Instance as BasePlugin)!;
        SubVersion = plugin.Metadata.Version;

        SubAssembly = SubPlugin.GetType().Assembly;
        SubTypes = AccessTools.GetTypesFromAssembly(SubAssembly);

        SubInjectedTypes = (Dictionary<string, Type>)AccessTools
            .PropertyGetter(SubTypes.FirstOrDefault(t => t.Name == "ComponentExtensions"), "RegisteredTypes")
            .Invoke(null, null)!;

        var submarineStatusType = SubTypes.First(t => t.Name == "SubmarineStatus");
        submergedInstance = AccessTools.Field(submarineStatusType, "instance");
        submergedElevators = AccessTools.Field(submarineStatusType, "elevators");

        var floorHandler = SubTypes.First(t => t.Name == "FloorHandler");
        getFloorHandler = AccessTools.Method(floorHandler, "GetFloorHandler", [typeof(PlayerControl)]);
        rpcRequestChangeFloor = AccessTools.Method(floorHandler, "RpcRequestChangeFloor");
        registerFloorOverride = AccessTools.Method(floorHandler, "RegisterFloorOverride");

        var ventPatchData = SubTypes.First(t => t.Name == "VentPatchData");
        inTransition = AccessTools.Property(ventPatchData, "InTransition");

        var customTaskTypes = SubTypes.First(t => t.Name == "CustomTaskTypes");
        var retrieveOxygenMask = AccessTools.Field(customTaskTypes, "RetrieveOxygenMask");
        var retTaskType = AccessTools.Field(customTaskTypes, "taskType");
        RetrieveOxygenMask = (TaskTypes)retTaskType.GetValue(retrieveOxygenMask.GetValue(null))!;

        var submarineOxygenSystem = SubTypes.First(t => t.Name == "SubmarineOxygenSystem");
        submarineOxygenSystemInstance = AccessTools.Property(submarineOxygenSystem, "Instance");
        repairDamage = AccessTools.Method(submarineOxygenSystem, "RepairDamage");
        var submergedExileController = SubTypes.First(t => t.Name == "SubmergedExileController");
        var submergedExileWrapUp = AccessTools.Method(submergedExileController, "WrapUpAndSpawn");

        var submarineElevator = SubTypes.First(t => t.Name == "SubmarineElevator");
        getInElevator = AccessTools.Method(submarineElevator, "GetInElevator", [typeof(PlayerControl)]);
        getMovementStageFromTime = AccessTools.Method(submarineElevator, "GetMovementStageFromTime");
        getSubElevatorSystem = AccessTools.Field(submarineElevator, "system");

        var elevatorConsole = SubTypes.First(t => t.Name == "ElevatorConsole");
        var canUse = AccessTools.Method(elevatorConsole, "CanUse");

        var submarineElevatorSystem = SubTypes.First(t => t.Name == "SubmarineElevatorSystem");
        upperDeckIsTargetFloor = AccessTools.Field(submarineElevatorSystem, "upperDeckIsTargetFloor");

        var compatType = typeof(ModCompatibility);
        var harmony = new Harmony("tou.submerged.patch");

        harmony.Patch(submergedExileWrapUp, null,
            new HarmonyMethod(AccessTools.Method(compatType, nameof(ExileRoleChangePostfix))));
        harmony.Patch(canUse, null, null,
            new HarmonyMethod(typeof(ModCompatibility), nameof(SubmergedElevatorTranspilerPatch)));

        SubLoaded = true;
        Logger<TownOfUsPlugin>.Message("Submerged was detected");
    }

    public static IEnumerable<CodeInstruction> SubmergedElevatorTranspilerPatch(
        IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Callvirt &&
                instruction.operand is MethodInfo { Name: "get_IsDead", DeclaringType.Name: "NetworkedPlayerInfo" })
            {
                found = true;
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Utilities.Extensions), nameof(Utilities.Extensions.IsGhostDead)));
            }
            else
            {
                yield return instruction;
            }
        }

        if (!found)
        {
            Logger<TownOfUsPlugin>.Error("Failed to find the IsDead call in SubmergedElevator transpiler");
        }
    }

    public static void CheckOutOfBoundsElevator(PlayerControl player)
    {
        if (!SubLoaded)
        {
            return;
        }

        if (!IsSubmerged())
        {
            return;
        }

        var elevator = GetPlayerElevator(player);
        if (!elevator.Item1)
        {
            return;
        }

        var currentFloor =
            (bool)upperDeckIsTargetFloor.GetValue(
                getSubElevatorSystem.GetValue(elevator.Item2))!; // true is top, false is bottom
        var playerFloor = player.transform.position.y > -7f; // true is top, false is bottom

        if (currentFloor != playerFloor)
        {
            ChangeFloor(currentFloor);
        }
    }

    public static void MoveDeadPlayerElevator(PlayerControl player)
    {
        if (!IsSubmerged())
        {
            return;
        }

        var elevator = GetPlayerElevator(player);
        if (!elevator.Item1)
        {
            return;
        }

        var movementStage = (int)getMovementStageFromTime.Invoke(elevator.Item2, null)!;
        if (movementStage >= 5)
        {
            // Fade to clear
            var topFloorTarget =
                (bool)upperDeckIsTargetFloor.GetValue(
                    getSubElevatorSystem.GetValue(elevator.Item2))!; // true is top, false is bottom
            var topIntendedTarget = player.transform.position.y > -7f; // true is top, false is bottom
            if (topFloorTarget != topIntendedTarget)
            {
                ChangeFloor(!topIntendedTarget);
            }
        }
    }

    public static Tuple<bool, object> GetPlayerElevator(PlayerControl player)
    {
        if (!IsSubmerged())
        {
            return Tuple.Create(false, (object)null!);
        }

        var elevatorList = (IList)submergedElevators.GetValue(submergedInstance.GetValue(null))!;
        foreach (var elevator in elevatorList)
        {
            if ((bool)getInElevator.Invoke(elevator, [player])!)
            {
                return Tuple.Create(true, elevator);
            }
        }

        return Tuple.Create(false, (object)null!);
    }

    public static void ExileRoleChangePostfix()
    {
        Coroutines.Start(WaitMeeting(ResetTimers));
        Coroutines.Start(WaitMeeting(GhostRoleBegin));
    }

    public static IEnumerator WaitMeeting(Action next)
    {
        while (!PlayerControl.LocalPlayer.moveable)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        while (HudManager.Instance.PlayerCam.transform.Find("SpawnInMinigame(Clone)") != null)
        {
            yield return null;
        }

        next();
    }

    public static void ResetTimers()
    {
        // Utils.ResetCustomTimers();
    }

    public static void GhostRoleBegin()
    {
        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is IGhostRole ghost && !ghost.Caught)
        {
            var startingVent =
                ShipStatus.Instance.AllVents[Random.RandomRangeInt(0, ShipStatus.Instance.AllVents.Count)];

            while (startingVent == ShipStatus.Instance.AllVents[0] || startingVent == ShipStatus.Instance.AllVents[14])
            {
                startingVent =
                    ShipStatus.Instance.AllVents[Random.RandomRangeInt(0, ShipStatus.Instance.AllVents.Count)];
            }

            ChangeFloor(startingVent.transform.position.y > -7f);

            var pos = new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f);

            PlayerControl.LocalPlayer.RpcSetPos(pos);
            PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(startingVent.Id);

            ghost.Setup = true;
        }
    }

    public static void GhostRoleFix(PlayerPhysics __instance)
    {
        if (SubLoaded && __instance.myPlayer.Data != null && __instance.myPlayer.Data.IsDead)
        {
            var player = __instance.myPlayer;

            if (player.Data.Role is IGhostRole ghost && ghost.GhostActive)
            {
                if (player.AmOwner)
                {
                    MoveDeadPlayerElevator(player);
                }
                else
                {
                    player.Collider.enabled = false;
                }

                var transform = __instance.transform;
                var position = transform.position;
                position.z = position.y / 1000;

                transform.position = position;
                __instance.myPlayer.gameObject.layer = 8;
            }
        }
    }

    public static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
    {
        if (!SubLoaded)
        {
            return obj.AddComponent<MissingBehaviour>();
        }

        var validType = SubInjectedTypes.TryGetValue(typeName, out var type);

        return validType
            ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>()!
            : obj.AddComponent<MissingBehaviour>();
    }

    public static void ChangeFloor(bool toUpper)
    {
        if (!SubLoaded)
        {
            return;
        }

        var handler = getFloorHandler.Invoke(null, [PlayerControl.LocalPlayer])!;
        rpcRequestChangeFloor.Invoke(handler, [toUpper]);
        registerFloorOverride.Invoke(handler, [toUpper]);
    }

    public static bool GetInTransition()
    {
        if (!SubLoaded)
        {
            return false;
        }

        return (bool)inTransition.GetValue(null)!;
    }

    public static void RepairOxygen()
    {
        if (!SubLoaded)
        {
            return;
        }

        try
        {
            ShipStatus.Instance.RpcUpdateSystem((SystemTypes)130, 64);
            repairDamage.Invoke(submarineOxygenSystemInstance.GetValue(null), [PlayerControl.LocalPlayer, 64]);
        }
        catch
        {
            // ignored
        }
    }

    public static bool IsSubmerged()
    {
        return SubLoaded && ShipStatus.Instance && ShipStatus.Instance.Type == SubmergedMapType;
    }

    public static bool IsLevelImpostor()
    {
        var map = GameOptionsManager.Instance.currentGameOptions.MapId;
        return LILoaded && ShipStatus.Instance && map == 7;
    }

    private static void InitLevelImpostor()
    {
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(LevelImpostorGuid, out var value))
        {
            return;
        }

        LIPlugin = (value.Instance as BasePlugin)!;
        LIAssembly = LIPlugin.GetType().Assembly;

        LITypes = AccessTools.GetTypesFromAssembly(LIAssembly);

        var mapLoader = LITypes.First(x => x.Name == "MapLoader");
        lastMapID = AccessTools.Field(mapLoader, "_lastMapID");
        currentMap = AccessTools.Property(mapLoader, "CurrentMap");

        var liMap = LITypes.First(x => x.Name == "LIMap");
        elements = AccessTools.Property(liMap, "elements");

        var liElement = LITypes.First(x => x.Name == "LIElement");
        liElementType = AccessTools.Property(liElement, "type");
        liElementName = AccessTools.Property(liElement, "name");

        var liDeathArea = LITypes.First(x => x.Name == "LIDeathArea");
        var killAllPlayersMethod = AccessTools.Method(liDeathArea, "KillAllPlayers");

        var console = LITypes.First(x => x.Name == "TriggerConsole");
        var canUseMethod = AccessTools.Method(console, "CanUse");

        MapObjectData = LITypes.First(x => x.Name == "MapObjectData");

        var compatType = typeof(ModCompatibility);
        var harmony = new Harmony("tou.levelimposter.patch");
        harmony.Patch(killAllPlayersMethod,
            new HarmonyMethod(AccessTools.Method(compatType, nameof(KillAllPlayersPrefix))));
        harmony.Patch(canUseMethod, new HarmonyMethod(AccessTools.Method(compatType, nameof(TriggerPrefix))),
            new HarmonyMethod(AccessTools.Method(compatType, nameof(TriggerPostfix))));

        LILoaded = true;
        Logger<TownOfUsPlugin>.Message("LevelImpostor was detected");
    }

    public static string GetLIVentType(Vent vent)
    {
        if (!IsLevelImpostor() || vent == null)
        {
            return string.Empty;
        }

        var map = currentMap.GetValue(null);
        var elems = (object[])elements.GetValue(map)!;
        var child = elems.FirstOrDefault(x => (string)liElementName.GetValue(x)! == vent.name);

        if (child == null)
        {
            return string.Empty;
        }

        return (string)liElementType.GetValue(child)!;
    }

    public static void TriggerPrefix(NetworkedPlayerInfo playerInfo, ref bool __state)
    {
        var playerControl = playerInfo.Object;

        if (playerControl.Data.Role is not IGhostRole { GhostActive: true })
        {
            return;
        }

        playerInfo.IsDead = false;
        __state = true;
    }

    public static void TriggerPostfix(NetworkedPlayerInfo playerInfo, ref bool __state)
    {
        if (__state)
        {
            playerInfo.IsDead = true;
        }
    }

    public static void KillAllPlayersPrefix(dynamic __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var playersToDie = __instance.CurrentPlayersIDs as List<byte>;

        if (playersToDie?.Count is null or 0)
        {
            return;
        }
    }
}