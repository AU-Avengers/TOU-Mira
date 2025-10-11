using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Options;

public static class TeamChatPatches
{
    private static TextMeshPro? _teamText;
    public static bool TeamChatActive;

    public static void ToggleTeamChat()
    {
        // WIP
        TeamChatActive = !TeamChatActive;
        if (!TeamChatActive)
        {
            HudManagerPatches.TeamChatButton.transform.Find("Inactive").gameObject.SetActive(true);
        }
        else
        {
            HudManager.Instance.Chat.Toggle();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendJailorChat)]
    public static void RpcSendJailorChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.IsJailed())
        {
            MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>Jailor</color>", text);
        }
        else if (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Jailor)</color>", text);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendJaileeChat)]
    public static void RpcSendJaileeChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.Data.Role is JailorRole || (PlayerControl.LocalPlayer.HasDied() &&
                                                                  OptionGroupSingleton<GeneralOptions>.Instance
                                                                      .TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Jailed)</color>", text);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendVampTeamChat)]
    public static void RpcSendVampTeamChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.Data.Role is VampireRole && player != PlayerControl.LocalPlayer) ||
            (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Vampire.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Vampire Chat)</color>",
                text);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendImpTeamChat)]
    public static void RpcSendImpTeamChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.IsImpostor() && player != PlayerControl.LocalPlayer) ||
            (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Impostor Chat)</color>",
                text);
        }
    }

    /* [HarmonyPatch(typeof(ChatController), nameof(ChatController.LateUpdate))]
    public static class LateUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (TeamChatActive)
            {
                var FreeChat = GameObject.Find("FreeChatInputField");
                var typeBg = FreeChat.transform.FindChild("Background");
                var typeText = FreeChat.transform.FindChild("Text");

                typeBg.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.6f);
                typeBg.GetComponent<ButtonRolloverHandler>().ChangeOutColor(new Color(0.2f, 0.1f, 0.1f, 0.6f));
                typeBg.GetComponent<ButtonRolloverHandler>().OverColor = new Color(0.6f, 0.1f, 0.1f, 1f);
                if (typeText.TryGetComponent<TextMeshPro>(out var txt))
                {
                    txt.color = Color.white;
                    txt.SetFaceColor(Color.white);
                }
            }
        }
    } */

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    public static class SetNamePatch
    {
        [HarmonyPostfix]
        public static void SetNamePostfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName,
            [HarmonyArgument(3)] Color color)
        {
            var player = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrDefault(x => x.Data.PlayerName == playerName);
            if (player == null)
            {
                return;
            }

            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
            if (genOpt.FFAImpostorMode && PlayerControl.LocalPlayer.IsImpostor() &&
                !PlayerControl.LocalPlayer.HasDied() &&
                !player.AmOwner && player.IsImpostor() && MeetingHud.Instance)
            {
                __instance.NameText.color = Color.white;
            }
            else if (color == Color.white &&
                     (player.AmOwner || player.Data.Role is MayorRole mayor && mayor.Revealed ||
                      PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) && PlayerControl.AllPlayerControls
                         .ToArray()
                         .FirstOrDefault(x => x.Data.PlayerName == playerName) && MeetingHud.Instance)
            {
                __instance.NameText.color = (player.GetRoleWhenAlive() is ICustomRole custom)
                    ? custom.RoleColor
                    : player.GetRoleWhenAlive().TeamColor;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    public static class TogglePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (PlayerControl.LocalPlayer == null ||
                PlayerControl.LocalPlayer.Data == null ||
                PlayerControl.LocalPlayer.Data.Role == null ||
                !ShipStatus.Instance ||
                (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
                 !TutorialManager.InstanceExists))
            {
                return;
            }

            try
            {
                if (__instance.IsOpenOrOpening)
                {
                    if (_teamText == null)
                    {
                        _teamText = Object.Instantiate(__instance.sendRateMessageText,
                            __instance.sendRateMessageText.transform.parent);
                        _teamText.text = string.Empty;
                        _teamText.color = TownOfUsColors.ImpSoft;
                    }

                    var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
                    _teamText.text = string.Empty;
                    if (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow &&
                        (genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true } || genOpt.VampireChat ||
                         Helpers.GetAlivePlayers().Any(x => x.Data.Role is JailorRole)))
                    {
                        _teamText.text = "Jailor, Impostor, and Vampire Chat can be seen here.";
                        _teamText.color = Color.white;
                    }

                    var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
                    // var FreeChat = GameObject.Find("FreeChatInputField");
                    var Background = ChatScreenContainer.transform.FindChild("Background");
                    // var bubbleItems = GameObject.Find("Items");
                    // var typeBg = FreeChat.transform.FindChild("Background");
                    // var typeText = FreeChat.transform.FindChild("Text");

                    if (TeamChatActive)
                    {
                        if (PlayerControl.LocalPlayer.TryGetModifier<JailedModifier>(out var jailMod) &&
                            !jailMod.HasOpenedQuickChat)
                        {
                            if (!__instance.quickChatMenu.IsOpen)
                            {
                                __instance.OpenQuickChat();
                            }

                            __instance.quickChatMenu.Close();
                            jailMod.HasOpenedQuickChat = true;
                        }

                        var ogChat = HudManager.Instance.Chat.chatButton;
                        ogChat.transform.Find("Inactive").gameObject.SetActive(true);
                        ogChat.transform.Find("Active").gameObject.SetActive(false);
                        ogChat.transform.Find("Selected").gameObject.SetActive(false);

                        Background.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
                        //typeBg.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.6f);
                        //typeText.GetComponent<TextMeshPro>().color = Color.white;
                        if (MeetingHud.Instance)
                        {
                            ChatScreenContainer.transform.localPosition =
                                HudManager.Instance.Chat.chatButton.transform.localPosition -
                                new Vector3(3.5133f + 4.33f * (Camera.main.orthographicSize / 3f), 4.576f);
                        }
                        else
                        {
                            ChatScreenContainer.transform.localPosition =
                                HudManager.Instance.Chat.chatButton.transform.localPosition -
                                new Vector3(3.5133f + 3.49f * (Camera.main.orthographicSize / 3f), 4.576f);
                        }

                        if ((PlayerControl.LocalPlayer.IsJailed() ||
                             PlayerControl.LocalPlayer.Data.Role is JailorRole) && _teamText != null)
                        {
                            _teamText.text = "Jailor Chat is Open. Only the Jailor and Jailee can see this.";
                            _teamText.color = TownOfUsColors.Jailor;
                        }
                        else if (PlayerControl.LocalPlayer.IsImpostor() &&
                                 genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true } &&
                                 !PlayerControl.LocalPlayer.Data.IsDead && _teamText != null)
                        {
                            _teamText.text = "Impostor Chat is Open. Only Impostors can see this.";
                            _teamText.color = TownOfUsColors.ImpSoft;
                        }
                        else if (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat &&
                                 !PlayerControl.LocalPlayer.Data.IsDead && _teamText != null)
                        {
                            _teamText.text = "Vampire Chat is Open. Only Vampires can see this.";
                            _teamText.color = TownOfUsColors.Vampire;
                        }
                        else if (_teamText != null)
                        {
                            _teamText.text = "Jailor, Impostor, and Vampire Chat can be seen here.";
                            _teamText.color = Color.white;
                        }
                        /* foreach (var bubble in bubbleItems.GetAllChilds())
                            {
                                bubble.gameObject.SetActive(true);
                                var bg = bubble.transform.Find("Background").gameObject;
                                if (bg != null)
                                {
                                    var sprite = bg.GetComponent<SpriteRenderer>();
                                    var color = sprite.color.SetAlpha(1f);
                                    if (color == Color.white || color == Color.black) bubble.gameObject.SetActive(false);
                                }
                            }
                        __instance.AlignAllBubbles(); */
                    }
                    else
                    {
                        /* foreach (var bubble in bubbleItems.GetAllChilds())
                        {
                            bubble.gameObject.SetActive(true);
                            var bg = bubble.transform.Find("Background").gameObject;
                            if (bg != null)
                            {
                                var sprite = bg.GetComponent<SpriteRenderer>();
                                var color = sprite.color.SetAlpha(1f);
                                if (color != Color.white && color != Color.black) bubble.gameObject.SetActive(false);
                            }
                        } */
                        Background.GetComponent<SpriteRenderer>().color = Color.white;
                        /* typeBg.GetComponent<SpriteRenderer>().color = Color.white;
                        typeBg.GetComponent<ButtonRolloverHandler>().ChangeOutColor(Color.white);
                        typeBg.GetComponent<ButtonRolloverHandler>().OverColor = new Color(0f, 1f, 0f, 1f);
                        if (typeText.TryGetComponent<TextMeshPro>(out var txt))
                        {
                            txt.color = new Color(0.6706f, 0.8902f, 0.8667f, 1f);
                            txt.SetFaceColor(new Color(0.6706f, 0.8902f, 0.8667f, 1f));
                        }
                        typeText.GetComponent<TextMeshPro>().color = new Color(0.6706f, 0.8902f, 0.8667f, 1f); */
                        ChatScreenContainer.transform.localPosition =
                            HudManager.Instance.Chat.chatButton.transform.localPosition -
                            new Vector3(3.5133f + 3.49f * (Camera.main.orthographicSize / 3f), 4.576f);
                    }
                }
                else if (TeamChatActive)
                {
                    ToggleTeamChat();
                }
            }
            catch
            {
                // Nothing Happens Here
            }
        }
    }

    /* [HarmonyPatch(typeof(ChatController), nameof(ChatController.AlignAllBubbles))]
    public static class AlignBubblesPatch
    {
        public static void Postfix(ChatController __instance)
        {
            var bubbleItems = GameObject.Find("Items");
            //float num = 0f;
            Il2CppSystem.Collections.Generic.List<PoolableBehavior> activeChildren = __instance.chatBubblePool.activeChildren;
            if (bubbleItems == null || bubbleItems.transform.GetChildCount() == 0 || activeChildren == null) return;
            if (TeamChatActive)
            {
                foreach (var bubble in bubbleItems.GetAllChilds())
                {
                    bubble.gameObject.SetActive(true);
                    var bg = bubble.transform.Find("Background").gameObject;
                    if (bg != null)
                    {
                        var sprite = bg.GetComponent<SpriteRenderer>();
                        var color = sprite.color.SetAlpha(1f);
                        if (color == Color.white || color == Color.black) bubble.gameObject.SetActive(false);
                    }
                }
                //var topPos = bubbleItems.transform.GetChild(0).transform.localPosition;
                for (int i = activeChildren.Count - 1; i >= 0; i--)
                {
                    var chatBubbleObj = activeChildren[i] as ChatBubble;
                    if (chatBubbleObj == null) continue;
                    ChatBubble chatBubble = chatBubbleObj!;
                    var bg = chatBubble.transform.Find("Background").gameObject;
                    if (bg != null)
                    {
                        var sprite = bg.GetComponent<SpriteRenderer>();
                        var color = sprite.color.SetAlpha(1f);
                        if (color == Color.white || color == Color.black)
                        {
                            chatBubble.gameObject.SetActive(false);
                            continue;
                        }
                    }
                }
            }
            else
            {
                foreach (var bubble in bubbleItems.GetAllChilds())
                {
                    bubble.gameObject.SetActive(true);
                    var bg = bubble.transform.Find("Background").gameObject;
                    if (bg != null)
                    {
                        var sprite = bg.GetComponent<SpriteRenderer>();
                        var color = sprite.color.SetAlpha(1f);
                        if (color != Color.white && color != Color.black) bubble.gameObject.SetActive(false);
                    }
                }
                //var topPos = bubbleItems.transform.GetChild(0).transform.localPosition;
                for (int i = activeChildren.Count - 1; i >= 0; i--)
                {
                    var chatBubbleObj = activeChildren[i] as ChatBubble;
                    if (chatBubbleObj == null) continue;
                    ChatBubble chatBubble = chatBubbleObj!;
                    var bg = chatBubble.transform.Find("Background").gameObject;
                    if (bg != null)
                    {
                        var sprite = bg.GetComponent<SpriteRenderer>();
                        var color = sprite.color.SetAlpha(1f);
                        if (color != Color.white && color != Color.black)
                        {
                            chatBubble.gameObject.SetActive(false);
                            continue;
                        }
                    }
                }
            }
            //float num2 = -0.3f;
            //__instance.scroller.SetYBoundsMin(Mathf.Min(0f, -num + __instance.scroller.Hitbox.bounds.size.y + num2));
        }
    } */
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Close))]
    public static class ClosePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (TeamChatActive)
            {
                ToggleTeamChat();
            }
        }
    }
}