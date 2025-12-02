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
using AmongUs.Data;
using TownOfUs.Modifiers;

namespace TownOfUs.Patches.Options;

public static class TeamChatPatches
{
    public static GameObject TeamChatButton;
    private static TextMeshPro? _teamText;
    public static bool TeamChatActive;
    public static bool ForceReset;
#pragma warning disable S2386
    public static List<PoolableBehavior> storedBubbles = new List<PoolableBehavior>();
    public static bool calledByChatUpdate;
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

    public static void ToggleTeamChat() // Also used to hide the custom chat when dying
    {
        TeamChatActive = !TeamChatActive;
        SoundManager.Instance.PlaySound(HudManager.Instance.Chat.quickChatButton.ClickSound, false, 1f, null);
        UpdateChat();
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
            calledByChatUpdate = true;
            chat.AlignAllBubbles();

            if (PrivateChatDot != null)
            {
                var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                sprite.enabled = false;
            }
        }
        else
        {
            foreach (var bubble in bubbleItems.GetAllChildren())
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
            calledByChatUpdate = true;
            chat.AlignAllBubbles();
            Background.GetComponent<SpriteRenderer>().color = Color.white;
            HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouChatAssets.NormalChatIdle.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouChatAssets.NormalChatHover.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = TouChatAssets.NormalChatOpen.LoadAsset();
            /* typeBg.GetComponent<SpriteRenderer>().color = Color.white;
            typeBg.GetComponent<ButtonRolloverHandler>().ChangeOutColor(Color.white);
            typeBg.GetComponent<ButtonRolloverHandler>().OverColor = new Color(0f, 1f, 0f, 1f);
            if (typeText.TryGetComponent<TextMeshPro>(out var txt))
            {
                txt.color = new Color(0.6706f, 0.8902f, 0.8667f, 1f);
                txt.SetFaceColor(new Color(0.6706f, 0.8902f, 0.8667f, 1f));
            }
            typeText.GetComponent<TextMeshPro>().color = new Color(0.6706f, 0.8902f, 0.8667f, 1f); */
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
        TeamChatButton.transform.localPosition = new Vector3(pos.x, pos.y + 1f, pos.z);
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
            if (!__instance.IsOpenOrOpening)
            {
                return;
            }

            if (PrivateChatDot != null &&
                (PlayerControl.LocalPlayer.IsLover() && MeetingHud.Instance == null || TeamChatActive))
            {
                var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                sprite.enabled = false;
            }

            if (ForceReset)
            {
                ForceReset = false;
                var chat = HudManager.Instance.Chat;
                var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
                var Background = ChatScreenContainer.transform.FindChild("Background");
                var bubbleItems = GameObject.Find("Items");
                foreach (var bubble in bubbleItems.GetAllChildren())
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
                calledByChatUpdate = true;
                chat.AlignAllBubbles();
                Background.GetComponent<SpriteRenderer>().color = Color.white;
                Sprite[] buttonArray = [ TouChatAssets.NormalChatIdle.LoadAsset(), TouChatAssets.NormalChatHover.LoadAsset(), TouChatAssets.NormalChatOpen.LoadAsset()];
                if (PlayerControl.LocalPlayer.IsLover() && MeetingHud.Instance == null)
                {
                    buttonArray = 
                        [ TouChatAssets.LoveChatIdle.LoadAsset(), TouChatAssets.LoveChatHover.LoadAsset(), TouChatAssets.LoveChatOpen.LoadAsset()];
                }
                HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = buttonArray[0];
                HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = buttonArray[1];
                HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = buttonArray[2];
            }

            if (TeamChatButton)
            {
                return;
            }

            CreateTeamChatButton();
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AlignAllBubbles))]
    public static class AlignBubblesPatch
    {
        public static void Prefix(ChatController __instance)
        {
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

            var isValid = MeetingHud.Instance &&
                          ((PlayerControl.LocalPlayer.IsJailed() || PlayerControl.LocalPlayer.Data.Role is JailorRole ||
                            (PlayerControl.LocalPlayer.IsImpostor() && genOpt is
                                { FFAImpostorMode: false, ImpostorChat.Value: true }) ||
                            (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat))
                           || !MeetingHud.Instance && PlayerControl.LocalPlayer.IsLover()) && calledByChatUpdate;

            if (!isValid)
            {
                return;
            }

            var bubbleItems = GameObject.Find("Items");
            var chat = HudManager.Instance.Chat;
            //float num = 0f;
            if (bubbleItems == null || bubbleItems.transform.GetChildCount() == 0) return;
            if (TeamChatActive)
            {
                if (storedBubbles.Count > 0)
                {
                    storedBubbles.Reverse(); // Messages gets added from last sent to first sent so we reverse the order
                    foreach (var bubble in storedBubbles)
                    {
                        chat.chatBubblePool.activeChildren.Add(bubble); // Add the stored bubbles
                    }
                    storedBubbles.Clear();
                }
                var children = chat.chatBubblePool.activeChildren.ToArray().ToList();
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
                //var topPos = bubbleItems.transform.GetChild(0).transform.localPosition;
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var chatBubbleObj = children[i].Cast<ChatBubble>();
                    if (chatBubbleObj == null) continue;
                    ChatBubble chatBubble = chatBubbleObj!;
                    var bg = chatBubble.transform.Find("Background").gameObject;
                    if (bg != null)
                    {
                        var sprite = bg.GetComponent<SpriteRenderer>();
                        var color = sprite.color.SetAlpha(1f);
                        if (color == Color.white || color == Color.black)
                        {
                            storedBubbles.Add(chatBubble); // Will contain all the custom chat bubbles
                            chat.chatBubblePool.activeChildren.Remove(chatBubble);
                            chatBubble.gameObject.SetActive(false);
                            continue;
                        }
                    }
                }
            }
            else
            {
                if (storedBubbles.Count > 0)
                {
                    storedBubbles.Reverse(); // Messages gets added from last sent to first sent so we reverse the order
                    foreach (var bubble in storedBubbles)
                    {
                        chat.chatBubblePool.activeChildren.Add(bubble); // Add the stored bubbles
                    }
                    storedBubbles.Clear();
                }
                var children = chat.chatBubblePool.activeChildren.ToArray().ToList();
                foreach (var bubble in bubbleItems.GetAllChildren())
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
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var chatBubbleObj = children[i].Cast<ChatBubble>();
                    if (chatBubbleObj == null) continue;
                    ChatBubble chatBubble = chatBubbleObj!;
                    var bg = chatBubble.transform.Find("Background").gameObject;
                    if (bg != null)
                    {
                        var sprite = bg.GetComponent<SpriteRenderer>();
                        var color = sprite.color.SetAlpha(1f);
                        if (color != Color.white && color != Color.black)
                        {
                            storedBubbles.Add(chatBubble); // Will contain all the normal chat bubbles
                            chat.chatBubblePool.activeChildren.Remove(chatBubble);
                            chatBubble.gameObject.SetActive(false);
                            continue;
                        }
                    }
                }
            }
            calledByChatUpdate = false;
            //float num2 = -0.3f;
            //__instance.scroller.SetYBoundsMin(Mathf.Min(0f, -num + __instance.scroller.Hitbox.bounds.size.y + num2));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendJailorChat)]
    public static void RpcSendJailorChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.IsJailed())
        {
            MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>Jailor</color>", text, bubbleType: BubbleType.Jailor);
        }
        else if (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Jailor)</color>", text, bubbleType: BubbleType.Jailor);
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
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Jailed)</color>", text, bubbleType: BubbleType.Jailor);
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
                text, bubbleType: BubbleType.Vampire);
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
                text, bubbleType: BubbleType.Impostor);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendLoveChat)]
    public static void RpcSendLoveChat(PlayerControl player, string text)
    {
        if (PlayerControl.LocalPlayer.IsLover() ||
            (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow))
        {
                MiscUtils.AddTeamChat(player.Data,
                    $"<color=#{TownOfUsColors.Lover.ToHtmlStringRGBA()}>{TouLocale.GetParsed("LoverChatTitle").Replace("<player>", player.Data.PlayerName)}</color>", text, blackoutText: false, bubbleType: BubbleType.Lover, onLeft: !player.AmOwner);
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
            if (genOpt.FFAImpostorMode && PlayerControl.LocalPlayer.IsImpostor() && !PlayerControl.LocalPlayer.HasDied() &&
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
                __instance.NameText.color = (player.GetRoleWhenAlive() is ICustomRole custom) ? custom.RoleColor : player.GetRoleWhenAlive().TeamColor;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    public static class AddChatPatch
    {
        [HarmonyPrefix]
        public static bool AddChatPrefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer, [HarmonyArgument(1)] string chatText,
        [HarmonyArgument(2)] bool censor) // I had no other choice... I did this to fix the bubbles being added in the wrong order to the chat when custom chat is opened (regular chat messages get added at the top of the conversation)
        {   // Feel free to change this fix if you find a better one - le killer
            if (!sourcePlayer || !PlayerControl.LocalPlayer)
            {
                return false;
            }
            NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
            NetworkedPlayerInfo data2 = sourcePlayer.Data;
            if (data2 == null || data == null || (data2.IsDead && !data.IsDead))
            {
                return false;
            }
            ChatBubble pooledBubble = __instance.GetPooledBubble();
            pooledBubble.transform.SetParent(__instance.scroller.Inner);
            pooledBubble.transform.localScale = Vector3.one;
            bool flag = sourcePlayer == PlayerControl.LocalPlayer;
            if (flag)
            {
                pooledBubble.SetRight();
            }
            else
            {
                pooledBubble.SetLeft();
            }
            bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
            pooledBubble.SetCosmetics(data2);
            __instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
            if (censor && DataManager.Settings.Multiplayer.CensorChat)
            {
                chatText = BlockedWords.CensorWords(chatText, false);
            }
            pooledBubble.SetText(chatText);
            pooledBubble.AlignChildren();
            if (!PlayerControl.LocalPlayer.Data.IsDead && TeamChatActive)
            {
                storedBubbles.Insert(0, pooledBubble);
                pooledBubble.gameObject.SetActive(false);
                if (__instance.chatBubblePool.activeChildren.Contains(pooledBubble))
                {
                    __instance.chatBubblePool.activeChildren.Remove(pooledBubble);
                }
            }
            __instance.AlignAllBubbles();

            if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
            {
                __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
            }
            if (!flag && !__instance.IsOpenOrOpening)
            {
                SoundManager.Instance.PlaySound(__instance.messageSound, false, 1f, null).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
                __instance.chatNotification.SetUp(sourcePlayer, chatText);
            }

            return false;
        }
    }
}
