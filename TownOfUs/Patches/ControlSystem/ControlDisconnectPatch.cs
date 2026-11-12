using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Interfaces;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Utilities.ControlSystem;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch(typeof(GameData))]
public static class ControlDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        if (ControlState.IsUsingAnyState(player.PlayerId, out var controllerId, out var isControlled, out var controlState) &&
            isControlled)
        {
            var controller = MiscUtils.PlayerById(controllerId);
            if (controller != null)
            {
                RemoteControlUtilities.RpcControllerEndControl(controller, player);
            }
            else
            {
                controlState.ClearControl(player.PlayerId);

                player.GetModifierOfType<IControlledModifier>()?.RemoveSelf();
            }
        }

        if (player.GetRemoteControl<IController>() is { Controlled: { } controlled })
        {
            RemoteControlUtilities.RpcControllerEndControl(player, controlled);
        }

        player.GetModifierOfType<IControlledModifier>()?.RemoveSelf();
    }
}
