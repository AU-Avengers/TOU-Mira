using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(GameData))]
public static class ChefDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (player.HasModifier<ChefServedModifier>())
        {
            ChefRole.DisconnectedServings.Add(player.PlayerId, player.Data.DefaultOutfit.ColorId);
        }
    }
}