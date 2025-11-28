using HarmonyLib;
using TownOfUs.Roles.Other;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(GameData))]
public static class DisconnectHandler2
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        SpectatorRole.TrackedSpectators.Remove(player.Data.PlayerName);
    }
}