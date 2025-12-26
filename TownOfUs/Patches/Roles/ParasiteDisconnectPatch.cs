using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(GameData))]
public static class ParasiteDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        if (ParasiteControlState.IsControlled(player.PlayerId, out var controllerId))
        {
            var controller = MiscUtils.PlayerById(controllerId);
            if (controller != null)
            {
                ParasiteRole.RpcParasiteEndControl(controller, player);
            }
            else
            {
                ParasiteControlState.ClearControl(player.PlayerId);

                if (player.TryGetModifier<ParasiteInfectedModifier>(out var mod))
                {
                    player.RemoveModifier(mod);
                }
            }
        }

        if (player.Data?.Role is ParasiteRole role && role.Controlled != null)
        {
            ParasiteRole.RpcParasiteEndControl(player, role.Controlled);
        }

        if (player.TryGetModifier<ParasiteInfectedModifier>(out var mod2))
        {
            player.RemoveModifier(mod2);
        }
    }
}