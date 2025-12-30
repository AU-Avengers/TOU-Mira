using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class SnarerVentPatches
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

        if (!VentSnareSystem.TryGetSnarerId(ventId, out var snarerId))
        {
            return;
        }

        if (!VentSnareSystem.IsEligibleToBeSnared(ventingPlayer))
        {
            return;
        }

        var snarer = MiscUtils.PlayerById(snarerId);
        if (snarer == null || snarer.Data?.Role is not SnarerRole)
        {
            VentSnareSystem.Remove(ventId);
            return;
        }

        SnarerRole.RpcSnarerTriggerSnare(snarer, ventId, ventingPlayer.PlayerId);
    }
}
