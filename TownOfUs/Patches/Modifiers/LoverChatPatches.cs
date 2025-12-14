using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class LoverChatPatches
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    public static bool SendChatPatch(ChatController __instance)
    {
        if (MeetingHud.Instance || ExileController.Instance != null || PlayerControl.LocalPlayer.Data.IsDead)
        {
            return true;
        }

        var text = __instance.freeChatField.Text.WithoutRichText();

        if (text.Length < 1 || text.Length > 100)
        {
            return true;
        }

        if (PlayerControl.LocalPlayer.HasModifier<LoverModifier>())
        {
            TeamChatPatches.RpcSendLoveChat(PlayerControl.LocalPlayer, text);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();

            return false;
        }

        return true;
    }
}