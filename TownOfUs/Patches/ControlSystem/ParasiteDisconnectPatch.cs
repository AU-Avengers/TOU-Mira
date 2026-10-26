using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches.ControlSystem;

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

        if (ParasiteRole.ControlState.IsControlled(player.PlayerId, out var controllerId))
        {
            var controller = MiscUtils.PlayerById(controllerId);
            if (controller != null)
            {
                ParasiteRole.RpcParasiteEndControl(controller, player, player.transform.position, false);
            }
            else
            {
                ParasiteRole.ControlState.ClearControl(player.PlayerId);

                player.TryRemoveModifier<ParasiteInfectedModifier>();
            }
        }

        if (player.Data?.Role is ParasiteRole { Controlled: { } controlled })
        {
            ParasiteRole.RpcParasiteEndControl(player, controlled, controlled.transform.position, false);
        }

        player.TryRemoveModifier<ParasiteInfectedModifier>();
    }
}