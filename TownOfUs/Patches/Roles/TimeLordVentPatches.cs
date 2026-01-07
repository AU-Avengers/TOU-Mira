using HarmonyLib;
using TownOfUs.Events.Crewmate;

namespace TownOfUs.Patches.Roles;

/// <summary>
/// Patches to record vent enter/exit events for Time Lord rewind system.
/// </summary>
[HarmonyPatch]
public static class TimeLordVentPatches
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    [HarmonyPostfix]
    public static void EnterVentPostfix(Vent __instance, PlayerControl pc)
    {
        if (pc != null && __instance != null && pc.AmOwner)
        {
            TimeLordEventHandlers.RecordVentEnter(pc, __instance);
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    [HarmonyPostfix]
    public static void ExitVentPostfix(PlayerPhysics __instance, int ventId)
    {
        var player = __instance.myPlayer;
        if (player == null || !player.AmOwner)
        {
            return;
        }

        var vent = ShipStatus.Instance?.AllVents?.FirstOrDefault(v => v != null && v.Id == ventId);
        if (vent != null)
        {
            TimeLordEventHandlers.RecordVentExit(player, vent);
        }
    }
}