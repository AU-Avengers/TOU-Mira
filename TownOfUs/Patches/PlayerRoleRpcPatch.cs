using HarmonyLib;
using Hazel;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class PlayerRoleRpcPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRoleRpc))]
    [HarmonyPrefix]
	public static bool HandleRoleRpcPrefix(PlayerControl __instance)
	{
        if (__instance.Data != null && __instance.Data.Role != null)
        {
            return true;
        }
        return false;
	}
}