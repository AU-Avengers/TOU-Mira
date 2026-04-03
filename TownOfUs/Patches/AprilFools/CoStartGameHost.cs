using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.AprilFools;

// Thanks Galster (https://github.com/Galster-dev), taken from https://github.com/Tommy-XL/Unlock-dlekS-ehT/blob/main/Patches/CoStartGameHostPatch.cs
[HarmonyPatch]
public static class CoStartGameHostPatch
{
    public static MethodBase TargetMethod()
    {
        return Helpers.GetStateMachineMoveNext<AmongUsClient>(nameof(AmongUsClient.CoStartGameHost))!;
    }

    public static bool Prefix(Il2CppObjectBase __instance, ref bool __result)
    {
        var wrapper = new StateMachineWrapper<AmongUsClient>(__instance);
        if (wrapper.GetState() != 0)
        {
            return true;
        }

        var client = wrapper.Instance;

        wrapper.SetState(-1);
        if (LobbyBehaviour.Instance)
        {
            LobbyBehaviour.Instance.Despawn();
        }

        if (ShipStatus.Instance)
        {
            wrapper.SetRecentReturn(null!);
            wrapper.SetState(2);
            __result = true;
            return false;
        }

        // removed dleks check as it's always false
        var num2 = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.MapId, 0, Constants.MapNames.Length - 1);
        wrapper.SetRecentReturn(client.ShipLoadingAsyncHandle = client.ShipPrefabs[num2].InstantiateAsync());
        wrapper.SetState(1);

        __result = true;
        return false;
    }
}