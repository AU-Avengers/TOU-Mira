using System.Reflection;
using HarmonyLib;
using TownOfUs.Roles;
using Object = Il2CppSystem.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class GhostRoleCanUsePatches
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Console), nameof(Console.CanUse));
        yield return AccessTools.Method(typeof(Ladder), nameof(Ladder.CanUse));
        yield return AccessTools.Method(typeof(PlatformConsole), nameof(PlatformConsole.CanUse));
        yield return AccessTools.Method(typeof(OpenDoorConsole), nameof(OpenDoorConsole.CanUse));
        yield return AccessTools.Method(typeof(DoorConsole), nameof(DoorConsole.CanUse));
        yield return AccessTools.Method(typeof(ZiplineConsole), nameof(ZiplineConsole.CanUse));
        yield return AccessTools.Method(typeof(DeconControl), nameof(DeconControl.CanUse));
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool CanUsePrefixPatch(Object __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc,
        ref bool __state)
    {
        __state = false;
        var playerControl = pc.Object;

        if (playerControl.Data.Role is IGhostRole ghost && ghost.GhostActive && pc.IsDead)
        {
            // Logger<TownOfUsPlugin>.Message($"CanUsePrefixPatch IsDead");
            pc.IsDead = false; // TODO: find a better way
            __state = true;
        }

        return true;
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void CanUsePostfixPatch(Object __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc,
        ref bool __state)
    {
        if (__state)
            // Logger<TownOfUsPlugin>.Message($"CanUsePostfixPatch IsDead");
        {
            pc.IsDead = true;
        }
    }/*

    [HarmonyPatch]
    public static class SpecificUsePatches
    {
        [HarmonyPatch(typeof(Ladder), nameof(Ladder.Use))]
        [HarmonyPrefix]
        public static bool LadderUsePrefix(Ladder __instance)
        {
            var data = PlayerControl.LocalPlayer.Data;
            __instance.CanUse(data, out var flag, out var _);
            if (flag)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcClimbLadder(__instance);
                __instance.CoolDown = __instance.MaxCoolDown;
            }

            return false;
        }

        [HarmonyPatch(typeof(ZiplineConsole), nameof(ZiplineConsole.Use))]
        [HarmonyPrefix]
        public static bool ZiplineUsePrefix(ZiplineConsole __instance)
        {
            var data = PlayerControl.LocalPlayer.Data;
            __instance.CanUse(data, out var flag, out var _);
            if (flag)
            {
                __instance.zipline.Use(__instance.atTop, __instance);
                __instance.CoolDown = __instance.MaxCoolDown;
            }

            return false;
        }
    }*/
}