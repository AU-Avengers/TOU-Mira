using System.Reflection;
using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Interfaces;

namespace TownOfUs.Patches.ControlSystem;


[HarmonyPatch]
public static class ControlledCanUsePatches
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

    [HarmonyPostfix]
    public static void CanUsePostfixPatch(
        Object __instance,
        [HarmonyArgument(0)] NetworkedPlayerInfo pc,
        [HarmonyArgument(1)] ref bool canUse,
        [HarmonyArgument(2)] ref bool couldUse)
    {
        if (pc == null || pc.Object == null)
        {
            return;
        }

        var player = pc.Object;

        var isControlled = player.TryGetModifierOfType<IControlledModifier>(out var control) &&
                           control.State.IsControlled(player.PlayerId, out _);

        if (!isControlled)
        {
            return;
        }

        if (player.IsInTargetingAnimState())
        {
            return;
        }

        canUse = true;
        couldUse = true;
    }
}