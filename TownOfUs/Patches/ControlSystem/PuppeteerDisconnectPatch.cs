using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch(typeof(GameData))]
public static class PuppeteerDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        if (PuppeteerRole.ControlState.IsControlled(player.PlayerId, out var controllerId))
        {
            var controller = MiscUtils.PlayerById(controllerId);
            if (controller != null)
            {
                PuppeteerRole.RpcPuppeteerEndControl(controller, player);
            }
            else
            {
                PuppeteerRole.ControlState.ClearControl(player.PlayerId);

                player.TryRemoveModifier<PuppeteerControlModifier>();
            }
        }

        if (player.Data?.Role is PuppeteerRole { Controlled: { } controlled })
        {
            PuppeteerRole.RpcPuppeteerEndControl(player, controlled);
        }

        player.TryRemoveModifier<PuppeteerControlModifier>();
    }
}