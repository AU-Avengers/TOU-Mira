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

    private const string PrivateBubblePrefix = "TOU_TeamChatBubble_";
    private const string PublicBubblePrefix = "TOU_PublicChatBubble";

    private static bool IsPrivateBubble(GameObject bubbleGo)
    {
        return bubbleGo != null && !DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && bubbleGo.name.StartsWith(PrivateBubblePrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static void PruneStoredBubbles()
    {
        for (var i = storedBubbles.Count - 1; i >= 0; i--)
        {
            var b = storedBubbles[i];
            if (b == null || !b || b.gameObject == null)
            {
                storedBubbles.RemoveAt(i);
            }
        }
    }

    private static void RestoreStoredBubbles(ChatController chat)
    {
        if (chat == null || chat.chatBubblePool == null)
        {
            storedBubbles.Clear();
            return;
        }

        PruneStoredBubbles();
        if (storedBubbles.Count == 0)
        {
            return;
        }

        storedBubbles.Reverse();
        foreach (var bubble in storedBubbles)
        {
            if (bubble == null || !bubble || bubble.gameObject == null)
            {
                continue;
            }

            if (!chat.chatBubblePool.activeChildren.Contains(bubble))
            {
                chat.chatBubblePool.activeChildren.Add(bubble);
            }

            bubble.gameObject.SetActive(true);
        }

        SortActiveChildrenByHierarchy(chat);
        storedBubbles.Clear();
    }

    private static void SortActiveChildrenByHierarchy(ChatController chat)
    {
        // We sometimes remove/re-add bubbles from the pool list to hide/show different chat "channels".
        // If we don't re-sort, the pool list order can drift from the actual UI hierarchy order,
        // causing messages to appear in the wrong order.
        if (chat == null || chat.chatBubblePool == null)
        {
            return;
        }

        var active = chat.chatBubblePool.activeChildren;
        if (active == null || active.Count <= 1)
        {
            return;
        }

        // Copy -> order -> rewrite list.
        var ordered = active.ToArray()
            .OrderBy(x => (x == null || !x || x.transform == null) ? int.MaxValue : x.transform.GetSiblingIndex())
            .ToArray();

        active.Clear();
        foreach (var item in ordered)
        {
            if (item == null || !item || item.gameObject == null)
            {
                continue;
            }
            active.Add(item);
        }
    }

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

        if (!HudManager.Instance.Chat.IsOpenOrOpening)
        {
            HudManager.Instance.Chat.Toggle();
            UpdateChat();
        }
    }

    public static void ForceNormalChat()
    {
        ForceReset = true;
        TeamChatActive = false;

        if (HudManager.InstanceExists && HudManager.Instance.Chat != null)
        {
            RestoreStoredBubbles(HudManager.Instance.Chat);

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
    }

    public static void UpdateChat()
    {
        var chat = HudManager.Instance.Chat;
        if (chat == null)
        {
            return;
        }

        // Keep pool/stored list clean before manipulating bubble visibility.
        RestoreStoredBubbles(chat);
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
                if (!IsPrivateBubble(bubble.gameObject))
                {
                    bubble.gameObject.SetActive(false);
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
                if (IsPrivateBubble(bubble.gameObject))
                {
                    bubble.gameObject.SetActive(false);
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
            if (!__instance.IsOpenOrOpening)
            {
                return;
            }

            // Ensure that opening chat reflects the currently-selected custom chat mode.
            if (TeamChatActive && !ForceReset)
            {
                UpdateChat();
            }

            if (PrivateChatDot != null &&
                (PlayerControl.LocalPlayer.IsLover() && MeetingHud.Instance == null || TeamChatActive))
            {
                var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                sprite.enabled = false;
            }

            if (!TeamChatActive || ForceReset)
            {
                ForceReset = false;
                var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
                var Background = ChatScreenContainer.transform.FindChild("Background");
                var bubbleItems = GameObject.Find("Items");
                foreach (var bubble in bubbleItems.GetAllChildren())
                {
                    bubble.gameObject.SetActive(true);
                    if (IsPrivateBubble(bubble.gameObject))
                    {
                        bubble.gameObject.SetActive(false);
                    }
                }
                var chat = HudManager.Instance.Chat;
                calledByChatUpdate = true;
                chat.AlignAllBubbles();
                Background.GetComponent<SpriteRenderer>().color = Color.white;
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
                            (PlayerControl.LocalPlayer.IsImpostorAligned() && genOpt is
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
            RestoreStoredBubbles(chat);
            if (TeamChatActive)
            {
                var children = chat.chatBubblePool.activeChildren.ToArray().ToList();
                foreach (var bubble in bubbleItems.GetAllChildren())
                {
                    bubble.gameObject.SetActive(true);
                    if (!IsPrivateBubble(bubble.gameObject))
                    {
                        bubble.gameObject.SetActive(false);
                    }
                }
                //var topPos = bubbleItems.transform.GetChild(0).transform.localPosition;
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var chatBubbleObj = children[i].Cast<ChatBubble>();
                    if (chatBubbleObj == null) continue;
                    ChatBubble chatBubble = chatBubbleObj!;
                    if (!IsPrivateBubble(chatBubble.gameObject))
                    {
                        storedBubbles.Add(chatBubble);
                        chat.chatBubblePool.activeChildren.Remove(chatBubble);
                        chatBubble.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                var children = chat.chatBubblePool.activeChildren.ToArray().ToList();
                foreach (var bubble in bubbleItems.GetAllChildren())
                {
                    bubble.gameObject.SetActive(true);
                    if (IsPrivateBubble(bubble.gameObject))
                    {
                        bubble.gameObject.SetActive(false);
                    }
                }
                //var topPos = bubbleItems.transform.GetChild(0).transform.localPosition;
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var chatBubbleObj = children[i].Cast<ChatBubble>();
                    if (chatBubbleObj == null) continue;
                    ChatBubble chatBubble = chatBubbleObj!;
                    if (IsPrivateBubble(chatBubble.gameObject))
                    {
                        storedBubbles.Add(chatBubble);
                        chat.chatBubblePool.activeChildren.Remove(chatBubble);
                        chatBubble.gameObject.SetActive(false);
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

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    public static class AddChatPatch
    {
        [HarmonyPostfix]
        public static void AddChatPostfix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer,
            [HarmonyArgument(1)] string chatText, [HarmonyArgument(2)] bool censor)
        {
            // "Do better" approach:
            // Don't reimplement the whole vanilla AddChat logic (brittle + can break with updates/other mods).
            // Let vanilla create the bubble, then selectively hide/store it if the user is viewing team chat
            // and re-sort the pool afterwards.
            try
            {
                if (__instance == null || __instance.chatBubblePool == null || !PlayerControl.LocalPlayer)
                {
                    return;
                }

                if (!TeamChatActive || PlayerControl.LocalPlayer.Data == null || PlayerControl.LocalPlayer.Data.IsDead)
                {
                    return;
                }

                var active = __instance.chatBubblePool.activeChildren;
                if (active == null || active.Count == 0)
                {
                    return;
                }

                var newest = active[active.Count - 1];
                var newestBubble = newest.TryCast<ChatBubble>();
                if (newestBubble == null || !newestBubble || newestBubble.gameObject == null)
                {
                    return;
                }

                // Ensure public bubbles aren't "sticky" as private due to pooling reuse.
                newestBubble.gameObject.name = PublicBubblePrefix;

                // While in team chat view, hide/store public chat messages so only private/team bubbles show.
                storedBubbles.Insert(0, newestBubble);
                newestBubble.gameObject.SetActive(false);
                active.Remove(newestBubble);
                SortActiveChildrenByHierarchy(__instance);
            }
            catch
            {
                // Swallow to avoid crashing a chat update path.
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.GetPooledBubble))]
    public static class GetPooledBubblePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ChatController __instance)
        {
            try
            {
                if (__instance == null || __instance.chatBubblePool == null)
                {
                    return;
                }

                // Remove invalid entries from the active list so ReclaimOldest() can't NRE.
                var active = __instance.chatBubblePool.activeChildren;
                for (var i = active.Count - 1; i >= 0; i--)
                {
                    var b = active[i];
                    if (b == null || !b || b.gameObject == null)
                    {
                        active.RemoveAt(i);
                    }
                }

                PruneStoredBubbles();
            }
            catch
            {
                // Swallow to avoid crashing a chat update path.
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ChatBubble __result)
        {
            // IMPORTANT: Chat bubbles are pooled/reused. If a bubble was previously used for a private/team chat
            // message, it may still be tagged as private. Vanilla meeting/system messages (votes/notes/celebrity/etc)
            // can then incorrectly appear inside the team chat view.
            //
            // Default every freshly-pooled bubble to "public", and let our private chat paths re-tag explicitly.
            try
            {
                if (__result == null || !__result || __result.gameObject == null)
                {
                    return;
                }

                __result.gameObject.name = PublicBubblePrefix;
            }
            catch
            {
                // Just avoid crashing the chat path lmao you get the idea
                // these are literally just here so the compiler stops yelling
                // at me about a catch with nothing in it
            }
        }
    }
}
