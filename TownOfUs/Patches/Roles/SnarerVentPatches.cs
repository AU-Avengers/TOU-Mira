using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class TrapperVentPatches
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    [HarmonyPostfix]
    public static void EnterVentPostfix(Vent __instance, PlayerControl pc)
    {
        if (pc == null || __instance == null || !pc.AmOwner || MeetingHud.Instance)
        {
            return;
        }

        TryTrigger(__instance.Id, pc);
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    [HarmonyPostfix]
    public static void ExitVentPostfix(PlayerPhysics __instance, int ventId)
    {
        var player = __instance?.myPlayer;
        if (player == null || !player.AmOwner || MeetingHud.Instance)
        {
            return;
        }

        TryTrigger(ventId, player);
    }

    private static void TryTrigger(int ventId, PlayerControl ventingPlayer)
    {
        if (TimeLordRewindSystem.IsRewinding)
        {
            return;
        }

        if (!VentTrapSystem.TryGetTraprId(ventId, out var trapperId))
        {
            return;
        }

        if (!VentTrapSystem.IsEligibleToBeTrapped(ventingPlayer))
        {
            return;
        }

        var trapper = MiscUtils.PlayerById(trapperId);
        if (trapper == null || trapper.Data?.Role is not TrapperRole)
        {
            VentTrapSystem.Remove(ventId);
            return;
        }

        TrapperRole.RpcTrapperTriggerTrap(trapper, ventId, ventingPlayer.PlayerId);
    }
}
