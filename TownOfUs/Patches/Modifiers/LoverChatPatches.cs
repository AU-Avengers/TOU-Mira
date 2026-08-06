using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Patches.Options;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class LoverChatPatches
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    public static bool SendChatPatch(ChatController __instance)
    {
        if (MeetingHud.Instance || ExileController.Instance || PlayerControl.LocalPlayer.Data.IsDead)
        {
            return true;
        }

        var text = __instance.freeChatField.Text.WithoutRichText();

        if (text.Length < 1 || text.Length > 301)
        {
            return true;
        }

        if (PlayerControl.LocalPlayer.HasModifier<LoverModifier>())
        {
            if (PlayerControl.LocalPlayer.HasModifier<ParasiteInfectedModifier>() ||
                PlayerControl.LocalPlayer.HasModifier<PuppeteerControlModifier>())
            {
                MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data,
                    $"<color=#{TownOfUsColors.Lover.ToHtmlStringRGBA()}>{TouLocale.GetParsed("LoverChatTitle").Replace("<player>", PlayerControl.LocalPlayer.Data.PlayerName)}</color>",
                    "You are under control! Your message cannot be sent.", blackoutText: false, bubbleType: BubbleType.Lover, onLeft: false);
            }
            else
            {
                TeamChatPatches.RpcSendLoveChat(PlayerControl.LocalPlayer, text);
            }

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();

            return false;
        }

        return true;
    }
}