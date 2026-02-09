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
    public static bool SplitChats =>
        LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.SeparateChatBubbles.Value;
    public static GameObject TeamChatButton;
    private static TextMeshPro? _teamText;
    public static bool TeamChatActive; // True if any team chat is active
    public static int CurrentChatIndex = -1; // Index of currently selected chat (-1 = normal chat)
    public static bool ForceReset;
#pragma warning disable S2386
    public static List<PoolableBehavior> storedBubbles = new List<PoolableBehavior>();
    public static bool calledByChatUpdate;
    public static GameObject? PrivateChatDot;

    internal const string PrivateBubblePrefix = "TOU_TeamChatBubble_";
    internal const string PublicBubblePrefix = "TOU_PublicChatBubble_";

    /// <summary>
    /// Registration system for extension team chats. Extensions can register their own team chat handlers.
    /// </summary>
    public static class ExtensionTeamChatRegistry
    {
        public static List<ExtensionTeamChatHandler> RegisteredHandlers { get; } = [];

        /// <summary>
        /// Register a team chat handler for extensions.
        /// </summary>
        public static void RegisterHandler(ExtensionTeamChatHandler handler)
        {
            if (handler != null && !RegisteredHandlers.Contains(handler))
            {
                RegisteredHandlers.Add(handler);
            }
        }

        /// <summary>
        /// Unregister a team chat handler.
        /// </summary>
        public static void UnregisterHandler(ExtensionTeamChatHandler handler)
        {
            RegisteredHandlers.Remove(handler);
        }

        /// <summary>
        /// Check if any registered extension team chat is available for the local player.
        /// </summary>
        public static bool IsAnyExtensionChatAvailable()
        {
            return RegisteredHandlers.Any(h => h.IsChatAvailable != null && h.IsChatAvailable());
        }

        /// <summary>
        /// Get the first available extension team chat handler.
        /// </summary>
        public static ExtensionTeamChatHandler? GetAvailableHandler()
        {
            return RegisteredHandlers.FirstOrDefault(h => h.IsChatAvailable != null && h.IsChatAvailable());
        }

        /// <summary>
        /// Get all available extension team chat handlers, sorted by priority.
        /// </summary>
        public static List<ExtensionTeamChatHandler> GetAllAvailableHandlers()
        {
            return RegisteredHandlers
                .Where(h => h.IsChatAvailable != null && h.IsChatAvailable())
                .OrderBy(h => h.Priority)
                .ToList();
        }

        /// <summary>
        /// Try to send a message through an extension team chat handler.
        /// Uses the highest priority (lowest number) available handler.
        /// </summary>
        public static bool TrySendExtensionChat(string message)
        {
            var handler = GetAllAvailableHandlers().FirstOrDefault();
            if (handler?.SendMessage != null)
            {
                handler.SendMessage(PlayerControl.LocalPlayer, message);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Represents an available team chat with its priority and send action.
    /// </summary>
    public sealed class AvailableTeamChat
    {
        public int Priority { get; set; }
        public Action<PlayerControl, string> SendAction { get; set; } = null!;
        public string DisplayName { get; set; } = string.Empty;
        public Color DisplayColor { get; set; } = Color.white;
        /// <summary>
        /// Optional: Background color for the chat screen when this chat is active.
        /// If null, uses the default team chat background color.
        /// </summary>
        public Color? BackgroundColor { get; set; }
        /// <summary>
        /// If true, this chat cannot be cycled away from and is always active when available.
        /// </summary>
        public bool IsForced { get; set; }
    }

    /// <summary>
    /// Helper class to get all available team chats (built-in + extensions) with priority.
    /// </summary>
    public static class TeamChatManager
    {
        private static bool _builtInChatsRegistered;
        private static readonly HashSet<int> UnreadChatPriorities = new HashSet<int>();

        /// <summary>
        /// Get the set of unread chat priorities. Used for checking unread status.
        /// </summary>
        public static HashSet<int> GetUnreadChatPriorities() => UnreadChatPriorities;

        /// <summary>
        /// Register all built-in team chats with the extension system.
        /// </summary>
        public static void RegisterBuiltInChats()
        {
            if (_builtInChatsRegistered)
            {
                return;
            }

            // Jailor chat - Priority 10, Forced
            var jailorHandler = new ExtensionTeamChatHandler
            {
                Priority = 10,
                IsForced = true,
                IsChatAvailable = () => MeetingHud.Instance != null && PlayerControl.LocalPlayer.Data.Role is JailorRole,
                SendMessage = (sender, msg) => RpcSendJailorChat(sender, msg),
                GetDisplayText = () => "Jail Chat",
                DisplayTextColor = TownOfUsColors.Jailor
            };
            ExtensionTeamChatRegistry.RegisterHandler(jailorHandler);

            // Jailee chat - Priority 20, Forced
            var jaileeHandler = new ExtensionTeamChatHandler
            {
                Priority = 20,
                IsForced = true,
                IsChatAvailable = () => MeetingHud.Instance != null && PlayerControl.LocalPlayer.IsJailed(),
                SendMessage = (sender, msg) => RpcSendJaileeChat(sender, msg),
                GetDisplayText = () => "Jail Chat",
                DisplayTextColor = TownOfUsColors.Jailor
            };
            ExtensionTeamChatRegistry.RegisterHandler(jaileeHandler);

            // Impostor chat - Priority 30
            var impostorHandler = new ExtensionTeamChatHandler
            {
                Priority = 30,
                IsForced = false,
                IsChatAvailable = () =>
                {
                    var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
                    return MeetingHud.Instance != null &&
                           PlayerControl.LocalPlayer.IsImpostorAligned() &&
                           genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true };
                },
                SendMessage = (sender, msg) => RpcSendImpTeamChat(sender, msg),
                GetDisplayText = () => "Impostor Chat",
                DisplayTextColor = TownOfUsColors.ImpSoft
            };
            ExtensionTeamChatRegistry.RegisterHandler(impostorHandler);

            // Vampire chat - Priority 40
            var vampireHandler = new ExtensionTeamChatHandler
            {
                Priority = 40,
                IsForced = false,
                IsChatAvailable = () =>
                {
                    var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
                    return MeetingHud.Instance != null &&
                           PlayerControl.LocalPlayer.Data.Role is VampireRole &&
                           genOpt.VampireChat;
                },
                SendMessage = (sender, msg) => RpcSendVampTeamChat(sender, msg),
                GetDisplayText = () => "Vampire Chat",
                DisplayTextColor = TownOfUsColors.Vampire
            };
            ExtensionTeamChatRegistry.RegisterHandler(vampireHandler);

            _builtInChatsRegistered = true;
        }

        /// <summary>
        /// Get all available team chats for the local player, sorted by priority.
        /// Filters out forced chats if we're not on a forced chat.
        /// </summary>
        public static List<AvailableTeamChat> GetAllAvailableChats(bool includeForced = true)
        {
            var chats = new List<AvailableTeamChat>();

            // Get all handlers (built-in + extensions)
            var allHandlers = ExtensionTeamChatRegistry.GetAllAvailableHandlers();
            
            foreach (var handler in allHandlers)
            {
                if (handler.SendMessage != null && (includeForced || !handler.IsForced))
                {
                    chats.Add(new AvailableTeamChat
                    {
                        Priority = handler.Priority,
                        SendAction = handler.SendMessage,
                        DisplayName = handler.GetDisplayText?.Invoke() ?? "Extension Chat",
                        DisplayColor = handler.DisplayTextColor ?? TownOfUsColors.ImpSoft,
                        BackgroundColor = handler.BackgroundColor,
                        IsForced = handler.IsForced
                    });
                }
            }

            return chats.OrderBy(c => c.Priority).ToList();
        }

        /// <summary>
        /// Mark a chat as having unread messages by its priority.
        /// </summary>
        public static void MarkChatAsUnread(int priority)
        {
            UnreadChatPriorities.Add(priority);
        }

        /// <summary>
        /// Mark a chat as read (clear unread status) by its priority.
        /// </summary>
        public static void MarkChatAsRead(int priority)
        {
            UnreadChatPriorities.Remove(priority);
        }

        /// <summary>
        /// Check if a chat has unread messages by its priority.
        /// </summary>
        public static bool HasUnreadMessages(int priority)
        {
            return UnreadChatPriorities.Contains(priority);
        }

        /// <summary>
        /// Clear all unread message flags.
        /// </summary>
        public static void ClearAllUnread()
        {
            UnreadChatPriorities.Clear();
        }

        /// <summary>
        /// Get the currently selected chat, or the first available chat if none selected.
        /// If there's an unread chat and no forced chat, auto-select the unread chat.
        /// </summary>
        public static AvailableTeamChat? GetCurrentChat(bool allowSelectionWhenInactive = false)
        {
            var chats = GetAllAvailableChats();
            if (chats.Count == 0)
            {
                CurrentChatIndex = -1;
                return null;
            }

            // If team chat is not active, only return null if we're not allowing selection
            // (This allows us to select a chat when activating for the first time)
            if (!TeamChatActive && !allowSelectionWhenInactive)
            {
                return null;
            }

            // If we're already on a chat (including forced), respect that choice
            if (CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count)
            {
                var currentChat = chats[CurrentChatIndex];
                // Clear unread for current chat when selected
                MarkChatAsRead(currentChat.Priority);
                return currentChat;
            }

            // If no chat is selected yet but team chat is active (or we're allowing selection), check for forced chat or unread chat
            var forcedChat = chats.FirstOrDefault(c => c.IsForced);
            if (forcedChat != null)
            {
                var forcedIndex = chats.FindIndex(c => c.Priority == forcedChat.Priority && c.DisplayName == forcedChat.DisplayName);
                if (forcedIndex >= 0)
                {
                    CurrentChatIndex = forcedIndex;
                    // Clear unread for forced chat when selected
                    MarkChatAsRead(forcedChat.Priority);
                    return chats[forcedIndex];
                }
            }

            // If no forced chat, check for unread messages
            if (UnreadChatPriorities.Count > 0)
            {
                // Find the highest priority unread chat that's available
                var unreadChat = chats.FirstOrDefault(c => UnreadChatPriorities.Contains(c.Priority));
                if (unreadChat != null)
                {
                    var unreadIndex = chats.FindIndex(c => c.Priority == unreadChat.Priority && c.DisplayName == unreadChat.DisplayName);
                    if (unreadIndex >= 0)
                    {
                        CurrentChatIndex = unreadIndex;
                        // Clear unread when auto-selected
                        MarkChatAsRead(unreadChat.Priority);
                        return chats[unreadIndex];
                    }
                }
            }

            // If no chat is selected or index is invalid, use the first available
            if (CurrentChatIndex < 0 || CurrentChatIndex >= chats.Count)
            {
                CurrentChatIndex = 0;
            }

            // Clear unread for the chat we're viewing
            if (CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count)
            {
                MarkChatAsRead(chats[CurrentChatIndex].Priority);
            }

            return chats[CurrentChatIndex];
        }

        /// <summary>
        /// Cycle to the next available chat. Returns true if cycled to a chat, false if cycled back to normal chat.
        /// Forced chats can cycle back to normal chat, but not to other team chats.
        /// </summary>
        public static bool CycleToNextChat()
        {
            var chats = GetAllAvailableChats();
            if (chats.Count == 0)
            {
                CurrentChatIndex = -1;
                TeamChatActive = false;
                return false;
            }

            // Check if we're currently on a forced chat
            var currentChat = CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count 
                ? chats[CurrentChatIndex] 
                : null;
            
            var isOnForcedChat = currentChat != null && currentChat.IsForced;

            // If we're on a forced chat, we can only cycle back to normal chat (not to other team chats)
            if (isOnForcedChat)
            {
                CurrentChatIndex = -1;
                TeamChatActive = false;
                return false;
            }

            // Cycle through non-forced chats
            var nonForcedChats = chats.Where(c => !c.IsForced).ToList();
            if (nonForcedChats.Count == 0)
            {
                CurrentChatIndex = -1;
                TeamChatActive = false;
                return false;
            }

            // Find current chat in non-forced list
            var currentIndexInNonForced = -1;
            if (currentChat != null && !currentChat.IsForced)
            {
                currentIndexInNonForced = nonForcedChats.FindIndex(c => c.Priority == currentChat.Priority && c.DisplayName == currentChat.DisplayName);
            }

            // If we're on the last non-forced chat, cycle back to normal chat
            if (currentIndexInNonForced >= 0 && currentIndexInNonForced >= nonForcedChats.Count - 1)
            {
                // Cycle back to normal chat
                CurrentChatIndex = -1;
                TeamChatActive = false;
                return false;
            }

            // Cycle to next non-forced chat
            if (currentIndexInNonForced >= 0)
            {
                currentIndexInNonForced++;
            }
            else
            {
                // Start at first non-forced chat
                currentIndexInNonForced = 0;
            }

            // Find the actual index in the full list
            var nextChat = nonForcedChats[currentIndexInNonForced];
            CurrentChatIndex = chats.FindIndex(c => c.Priority == nextChat.Priority && c.DisplayName == nextChat.DisplayName);
            TeamChatActive = true;
            return true;
        }

        /// <summary>
        /// Send a message through the currently selected chat.
        /// </summary>
        public static bool SendMessage(string message)
        {
            var currentChat = GetCurrentChat();
            if (currentChat != null)
            {
                currentChat.SendAction(PlayerControl.LocalPlayer, message);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Handler for extension team chats. Extensions should create instances of this to register their team chat.
    /// </summary>
    public sealed class ExtensionTeamChatHandler
    {
        /// <summary>
        /// Function to check if this team chat is available for the local player.
        /// Should return true if the player can use this team chat.
        /// </summary>
        public Func<bool>? IsChatAvailable { get; set; }

        /// <summary>
        /// Function to send a message through this team chat.
        /// Parameters: (sender, message)
        /// </summary>
        public Action<PlayerControl, string>? SendMessage { get; set; }

        /// <summary>
        /// Optional: Function to get the display text when this chat is active.
        /// Should return the text to display, or null to use default.
        /// </summary>
        public Func<string>? GetDisplayText { get; set; }

        /// <summary>
        /// Optional: Color for the display text.
        /// </summary>
        public Color? DisplayTextColor { get; set; }

        /// <summary>
        /// Optional: Background color for the chat screen when this chat is active.
        /// If null, uses the default team chat background color.
        /// </summary>
        public Color? BackgroundColor { get; set; }

        /// <summary>
        /// Optional: Function to check if dead players can see this chat (when "The Dead Know" is enabled).
        /// Parameters: (deadPlayer)
        /// </summary>
        public Func<PlayerControl, bool>? CanDeadPlayerSee { get; set; }

        /// <summary>
        /// Priority for this chat when multiple chats are available. Lower numbers = higher priority.
        /// Default is 100. Built-in chats use: Jailor=10, Jailee=20, Impostor=30, Vampire=40.
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// If true, this chat cannot be cycled away from and is always active when available.
        /// Use this for critical chats like Jailor that should always be accessible.
        /// </summary>
        public bool IsForced { get; set; }
    }

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
        // Ensure built-in chats are registered
        TeamChatManager.RegisterBuiltInChats();

        var availableChats = TeamChatManager.GetAllAvailableChats();
        
        if (availableChats.Count == 0)
        {
            // No chats available, just toggle off
            TeamChatActive = false;
            CurrentChatIndex = -1;
        }
        else if (!TeamChatActive)
        {
            // First time activating - select the first available chat (or forced chat, or unread chat)
            var currentChat = TeamChatManager.GetCurrentChat(allowSelectionWhenInactive: true);
            if (currentChat != null)
            {
                TeamChatActive = true;
            }
        }
        else
        {
            // Already active - cycle to next chat (may cycle back to normal chat)
            TeamChatManager.CycleToNextChat();
        }

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
        CurrentChatIndex = -1;

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
        // Ensure built-in chats are registered
        TeamChatManager.RegisterBuiltInChats();

        var chat = HudManager.Instance.Chat;
        chat.UpdateChatMode();
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

            // Ensure built-in chats are registered
            TeamChatManager.RegisterBuiltInChats();

            // Get currently selected chat
            var currentChat = TeamChatManager.GetCurrentChat();
            var availableChats = TeamChatManager.GetAllAvailableChats();

            // Set background color based on current chat's custom color, or use default
            var backgroundColor = currentChat?.BackgroundColor ?? new Color(0.2f, 0.1f, 0.1f, 0.8f);
            Background.GetComponent<SpriteRenderer>().color = backgroundColor;

            HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouChatAssets.TeamChatIdle.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouChatAssets.TeamChatHover.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = TouChatAssets.TeamChatOpen.LoadAsset();

            if (currentChat != null && _teamText != null)
            {
                // Forced chats always show simple message (can't cycle to other team chats)
                if (currentChat.IsForced)
                {
                    _teamText.text = $"{currentChat.DisplayName} is Active. Messages will be sent to this chat.";
                }
                else
                {
                    // Count only non-forced chats for the cycle indicator
                    var nonForcedChats = availableChats.Where(c => !c.IsForced).ToList();
                    if (nonForcedChats.Count > 1)
                    {
                        var currentIndexInNonForced = nonForcedChats.FindIndex(c => c.Priority == currentChat.Priority && c.DisplayName == currentChat.DisplayName);
                        var chatNumber = currentIndexInNonForced >= 0 ? currentIndexInNonForced + 1 : 1;
                        _teamText.text = $"{currentChat.DisplayName} is Active ({chatNumber}/{nonForcedChats.Count}). Press button to cycle.";
                    }
                    else
                    {
                        _teamText.text = $"{currentChat.DisplayName} is Active. Messages will be sent to this chat.";
                    }
                }
                _teamText.color = currentChat.DisplayColor;
            }
            else if (_teamText != null)
            {
                // Fallback for dead players or when no chats are available
                if (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && genOpt.TheDeadKnow)
                {
                    _teamText.text = "Jailor, Impostor, and Vampire Chat can be seen here.";
                    _teamText.color = Color.white;
                }
                else
                {
                    _teamText.text = string.Empty;
                }
            }
            foreach (var bubble in bubbleItems.GetAllChildren())
            {
                bubble.gameObject.SetActive(true);
                if (SplitChats && !IsPrivateBubble(bubble.gameObject))
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
                if (SplitChats && IsPrivateBubble(bubble.gameObject))
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
                    if (SplitChats && IsPrivateBubble(bubble.gameObject))
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

            // Check extension team chats
            if (!isValid && MeetingHud.Instance && calledByChatUpdate)
            {
                isValid = ExtensionTeamChatRegistry.IsAnyExtensionChatAvailable();
            }

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
                    if (SplitChats && !IsPrivateBubble(bubble.gameObject))
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
                    if (SplitChats && !IsPrivateBubble(chatBubble.gameObject))
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
                    if (SplitChats && IsPrivateBubble(bubble.gameObject))
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
                    if (SplitChats && IsPrivateBubble(chatBubble.gameObject))
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
        var shouldMarkUnread = false;
        if (PlayerControl.LocalPlayer.IsJailed())
        {
            MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("TouRoleJailor")}</color>",
                text, bubbleType: BubbleType.Jailor, onLeft: !player.AmOwner);
            shouldMarkUnread = true;
        }
        else if (PlayerControl.LocalPlayer.Data.Role is JailorRole || DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("JailorChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, bubbleType: BubbleType.Jailor, onLeft: !player.AmOwner);
            shouldMarkUnread = true;
        }

        // Mark as unread if message was received and chat is not currently active
        if (shouldMarkUnread && MeetingHud.Instance != null)
        {
            var chats = TeamChatManager.GetAllAvailableChats();
            var hasForcedChat = chats.Any(c => c.IsForced);
            // Only mark as unread if not currently viewing this chat and no forced chat is active
            var currentChat = CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count ? chats[CurrentChatIndex] : null;
            if ((!TeamChatActive || currentChat == null || currentChat.Priority != 10) && !hasForcedChat)
            {
                TeamChatManager.MarkChatAsUnread(10); // Jailor chat priority
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendJaileeChat)]
    public static void RpcSendJaileeChat(PlayerControl player, string text)
    {
        var shouldMarkUnread = false;
        if (PlayerControl.LocalPlayer.Data.Role is JailorRole || PlayerControl.LocalPlayer.IsJailed() || (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) &&
                                                                 OptionGroupSingleton<GeneralOptions>.Instance
                                                                     .TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("JaileeChatTitle").Replace("<player>", player.Data.PlayerName)}</color>", text,
                bubbleType: BubbleType.Jailor, onLeft: !player.AmOwner);
            shouldMarkUnread = true;
        }

        // Mark as unread if message was received and chat is not currently active
        if (shouldMarkUnread && MeetingHud.Instance != null)
        {
            var chats = TeamChatManager.GetAllAvailableChats();
            var hasForcedChat = chats.Any(c => c.IsForced);
            // Only mark as unread if not currently viewing this chat and no forced chat is active
            var currentChat = CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count ? chats[CurrentChatIndex] : null;
            if ((!TeamChatActive || currentChat == null || currentChat.Priority != 20) && !hasForcedChat)
            {
                TeamChatManager.MarkChatAsUnread(20); // Jailee chat priority
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendVampTeamChat)]
    public static void RpcSendVampTeamChat(PlayerControl player, string text)
    {
        var shouldMarkUnread = false;
        if ((PlayerControl.LocalPlayer.Data.Role is VampireRole) ||
            (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.Vampire.ToHtmlStringRGBA()}>{TouLocale.GetParsed("VampireChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, bubbleType: BubbleType.Vampire, onLeft: !player.AmOwner);
            shouldMarkUnread = true;
        }

        // Mark as unread if message was received and chat is not currently active
        if (shouldMarkUnread && MeetingHud.Instance != null)
        {
            var chats = TeamChatManager.GetAllAvailableChats();
            var hasForcedChat = chats.Any(c => c.IsForced);
            // Only mark as unread if not currently viewing this chat and no forced chat is active
            var currentChat = CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count ? chats[CurrentChatIndex] : null;
            if ((!TeamChatActive || currentChat == null || currentChat.Priority != 40) && !hasForcedChat)
            {
                TeamChatManager.MarkChatAsUnread(40); // Vampire chat priority
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendImpTeamChat)]
    public static void RpcSendImpTeamChat(PlayerControl player, string text)
    {
        var shouldMarkUnread = false;
        if ((PlayerControl.LocalPlayer.IsImpostorAligned()) ||
            (DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data,
                $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{TouLocale.GetParsed("ImpostorChatTitle").Replace("<player>", player.Data.PlayerName)}</color>",
                text, bubbleType: BubbleType.Impostor, onLeft: !player.AmOwner);
            shouldMarkUnread = true;
        }

        // Mark as unread if message was received and chat is not currently active
        if (shouldMarkUnread && MeetingHud.Instance != null)
        {
            var chats = TeamChatManager.GetAllAvailableChats();
            var hasForcedChat = chats.Any(c => c.IsForced);
            // Only mark as unread if not currently viewing this chat and no forced chat is active
            var currentChat = CurrentChatIndex >= 0 && CurrentChatIndex < chats.Count ? chats[CurrentChatIndex] : null;
            if ((!TeamChatActive || currentChat == null || currentChat.Priority != 30) && !hasForcedChat)
            {
                TeamChatManager.MarkChatAsUnread(30); // Impostor chat priority
            }
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
                UpdateChat();

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
