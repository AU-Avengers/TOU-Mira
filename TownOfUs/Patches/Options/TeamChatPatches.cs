using HarmonyLib;
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
using UnityEngine.UI;
using InnerNet;
using TownOfUs.Modifiers;

namespace TownOfUs.Patches.Options;

public static class TeamChatPatches
{
    public static GameObject TeamChatButton;
    private static TextMeshPro? _teamText;
    public static bool TeamChatActive;
    public static GameObject? PrivateChatDot;

    public static class CustomChatData
    {
        public static List<ChatHolder> CustomChatHolders { get; set; } = [];

        public static void Clear()
        {
            CustomChatHolders.Clear();
        }

        public static void AddChatHolder(string infoBlurb = "This is a custom chat!",
            string titleFormat = "<player> (Chat)", Color? infoColor = null, Color? titleColor = null,
            Color? msgBgColor = null, Color? bgColor = null, Sprite? spriteBubble = null, Sprite? btnIdle = null,
            Sprite? btnHover = null, Sprite? btnOpen = null, Func<bool>? canSeeChat = null,
            Func<bool>? canUseChat = null)
        {
            CustomChatHolders.Add(new ChatHolder
            {
                InformationBlurb = infoBlurb,
                ChatTitleFormat = titleFormat,
                InfoBlurbColor = infoColor ?? Color.white,
                ChatMessageTitleColor = titleColor,
                ChatMessageBgColor = msgBgColor,
                ChatBgColor = bgColor,
                ChatBubbleSprite = spriteBubble ?? TouChatAssets.NormalBubble.LoadAsset(),
                ButtonIdleSprite = btnIdle ?? TouChatAssets.NormalChatIdle.LoadAsset(),
                ButtonHoverSprite = btnHover ?? TouChatAssets.NormalChatHover.LoadAsset(),
                ButtonOpenSprite = btnOpen ?? TouChatAssets.NormalChatOpen.LoadAsset(),
                ChatVisible = () => (canSeeChat == null || canSeeChat()),
                ChatUsable = () => (canSeeChat == null || canSeeChat()) && (canUseChat == null || canUseChat())
            });
        }

        public sealed class ChatHolder
        {
            public string InformationBlurb { get; set; }
            public string ChatTitleFormat { get; set; }
            public Color InfoBlurbColor { get; set; }
            public Color? ChatMessageTitleColor { get; set; }
            public Color? ChatMessageBgColor { get; set; }
            public Color? ChatBgColor { get; set; }
            public Sprite ChatBubbleSprite { get; set; }
            public Sprite ButtonIdleSprite { get; set; }
            public Sprite ButtonHoverSprite { get; set; }
            public Sprite ButtonOpenSprite { get; set; }
            public Func<bool> ChatVisible { get; set; }
            public Func<bool> ChatUsable { get; set; }
        }
    }

    public static void ToggleTeamChat()
    {
        // WIP
        TeamChatActive = !TeamChatActive;
        if (!TeamChatActive)
        {
            HudManagerPatches.TeamChatButton.transform.Find("Inactive").gameObject.SetActive(true);
        }
    }

    public static void UpdateChat()
    {
        var chat = HudManager.Instance.Chat;
        if (_teamText == null)
        {
            _teamText = Object.Instantiate(chat.sendRateMessageText,
                chat.sendRateMessageText.transform.parent);
            _teamText.text = string.Empty;
            _teamText.color = TownOfUsColors.ImpSoft;
            _teamText.gameObject.SetActive(true);
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        _teamText.text = string.Empty;
        if (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && genOpt.TheDeadKnow &&
            (genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true } || genOpt.VampireChat ||
             Helpers.GetAlivePlayers().Any(x => x.Data.Role is JailorRole)))
        {
            _teamText.text = "Jailor, Impostor, and Vampire Chat can be seen here.";
            _teamText.color = Color.white;
        }

        var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
        // var FreeChat = GameObject.Find("FreeChatInputField");
        var Background = ChatScreenContainer.transform.FindChild("Background");
        var bubbleItems = GameObject.Find("Items");
        // var typeBg = FreeChat.transform.FindChild("Background");
        // var typeText = FreeChat.transform.FindChild("Text");

        if (TeamChatActive)
        {
            if (PlayerControl.LocalPlayer.TryGetModifier<JailedModifier>(out var jailMod) && !jailMod.HasOpenedQuickChat)
            {
                if (!chat.quickChatMenu.IsOpen) chat.OpenQuickChat();
                chat.quickChatMenu.Close();
                jailMod.HasOpenedQuickChat = true;
            }

            Background.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
            HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouChatAssets.TeamChatIdle.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouChatAssets.TeamChatHover.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = TouChatAssets.TeamChatOpen.LoadAsset();

            if ((PlayerControl.LocalPlayer.IsJailed() ||
                 PlayerControl.LocalPlayer.Data.Role is JailorRole) && _teamText != null)
            {
                _teamText.text = "Jail Chat is Open. Only the Jailor and Jailee can see this.";
                _teamText.color = TownOfUsColors.Jailor;
            }
            else if (PlayerControl.LocalPlayer.IsImpostorAligned() &&
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
            foreach (var bubble in bubbleItems.GetAllChildren())
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
            chat.AlignAllBubbles();

            if (PrivateChatDot != null)
            {
                var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                sprite.enabled = false;
            }
        }
        else
        {
            HudManager.Instance.Chat.Toggle();
        }
    }

    public static void CreateTeamChatButton()
    {
        if (TeamChatButton)
        {
            return;
        }

        var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
        var BanMenu = ChatScreenContainer.transform.FindChild("BanMenuButton");

        TeamChatButton = Object.Instantiate(BanMenu.gameObject, BanMenu.transform.parent);
        TeamChatButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        TeamChatButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(ToggleTeamChat));
        TeamChatButton.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatSwitch.LoadAsset();
        TeamChatButton.name = "FactionChat";
        var pos = BanMenu.transform.localPosition;
        TeamChatButton.transform.localPosition = new Vector3(pos.x, pos.y + 0.7f, pos.z);
    }

    public static void CreateTeamChatBubble()
    {
        var obj = HudManager.Instance.Chat.chatNotifyDot.gameObject;
        PrivateChatDot = Object.Instantiate(obj, obj.transform.parent);
        PrivateChatDot.transform.localPosition -= new Vector3(0f, 0.325f, 0f);
        PrivateChatDot.transform.localScale -= new Vector3(0.2f, 0.2f, 0f);
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
                    if (PrivateChatDot != null &&
                        (PlayerControl.LocalPlayer.IsLover() && PrivateChatDot.GetComponent<SpriteRenderer>().sprite ==
                            TouChatAssets.LoveBubble.LoadAsset() || TeamChatActive))
                    {
                        var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                        sprite.enabled = false;
                    }

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
                        else if (PlayerControl.LocalPlayer.IsImpostorAligned() &&
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

    [MethodRpc((uint)TownOfUsRpc.SendJailorChat)]
    public static void RpcSendJailorChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.IsJailed())
        {
            MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("TouRoleJailor")}</color>",
                text, bubbleType: BubbleType.Jailor, onLeft: !player.AmOwner);
        }
        else if (PlayerControl.LocalPlayer.Data.Role is JailorRole || DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("JailorChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, bubbleType: BubbleType.Jailor, onLeft: !player.AmOwner);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendJaileeChat)]
    public static void RpcSendJaileeChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.Data.Role is JailorRole || PlayerControl.LocalPlayer.IsJailed() || (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) &&
                                                                  OptionGroupSingleton<GeneralOptions>.Instance
                                                                      .TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("JaileeChatTitle").Replace("<player>", player.Data.PlayerName)}</color>", text,
                bubbleType: BubbleType.Jailor, onLeft: !player.AmOwner);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendVampTeamChat)]
    public static void RpcSendVampTeamChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.Data.Role is VampireRole) ||
            (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Vampire.ToHtmlStringRGBA()}>{TouLocale.GetParsed("VampireChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, bubbleType: BubbleType.Vampire, onLeft: !player.AmOwner);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendImpTeamChat)]
    public static void RpcSendImpTeamChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.IsImpostorAligned()) ||
            (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{TouLocale.GetParsed("ImpostorChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, bubbleType: BubbleType.Impostor, onLeft: !player.AmOwner);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendLoveChat)]
    public static void RpcSendLoveChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.IsLover() ||
            (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Lover.ToHtmlStringRGBA()}>{TouLocale.GetParsed("LoverChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, blackoutText: false, bubbleType: BubbleType.Lover, onLeft: !player.AmOwner);
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    public static class SetNamePatch
    {
        [HarmonyPostfix]
        public static void SetNamePostfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName, [HarmonyArgument(3)] Color color)
        {
            var player = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrDefault(x => x.Data.PlayerName == playerName);
            if (player == null) return;
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
            if (genOpt.FFAImpostorMode && PlayerControl.LocalPlayer.IsImpostorAligned() && !DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) &&
                !player.AmOwner && player.IsImpostorAligned() && MeetingHud.Instance)
            {
                __instance.NameText.color = Color.white;
            }
            else if (color == Color.white &&
                     (player.AmOwner || player.Data.Role is MayorRole mayor && mayor.Revealed ||
                      DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && genOpt.TheDeadKnow) && PlayerControl.AllPlayerControls
                         .ToArray()
                         .FirstOrDefault(x => x.Data.PlayerName == playerName) && MeetingHud.Instance)
            {
                __instance.NameText.color = (player.GetRoleWhenAlive() is ICustomRole custom) ? custom.RoleColor : player.GetRoleWhenAlive().TeamColor;
            }
        }
    }
}
