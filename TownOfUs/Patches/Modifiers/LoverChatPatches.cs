using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class LoverChatPatches
{
    private static bool LoverMessage;
    public static bool overrideMessages;

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    public static bool SendChatPatch(ChatController __instance)
    {
        if (MeetingHud.Instance || ExileController.Instance != null || PlayerControl.LocalPlayer.Data.IsDead ||
            overrideMessages)
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
            LoverMessage = true;
            RpcSendLoveChat(PlayerControl.LocalPlayer, text);
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();

            return false;
        }

        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.SendLoveChat)]
    public static void RpcSendLoveChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.IsLover() && player != PlayerControl.LocalPlayer) ||
            (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            LoverMessage = true;
            if (player != PlayerControl.LocalPlayer)
            {
                HudManager.Instance.Chat.AddChat(player, text);
            }
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    [HarmonyPostfix]
    public static void SetNamePatch(ChatBubble __instance, [HarmonyArgument(0)] string playerName)
    {
        if (LoverMessage && !overrideMessages)
        {
            __instance.NameText.color = TownOfUsColors.Lover;
            __instance.NameText.text = playerName + " (Lover)";
            LoverMessage = false;
        }
    }
}