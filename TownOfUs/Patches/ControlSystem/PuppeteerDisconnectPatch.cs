using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

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

        if (PuppeteerControlState.IsControlled(player.PlayerId, out var controllerId))
        {
            var controller = MiscUtils.PlayerById(controllerId);
            if (controller != null)
            {
                PuppeteerRole.RpcPuppeteerEndControl(controller, player);
            }
            else
            {
                PuppeteerControlState.ClearControl(player.PlayerId);

                if (player.TryGetModifier<PuppeteerControlModifier>(out var mod))
                {
                    player.RemoveModifier(mod);
                }
            }
        }

        if (player.Data?.Role is PuppeteerRole role && role.Controlled != null)
        {
            PuppeteerRole.RpcPuppeteerEndControl(player, role.Controlled);
        }

        if (player.TryGetModifier<PuppeteerControlModifier>(out var mod2))
        {
            player.RemoveModifier(mod2);
        }
    }
}