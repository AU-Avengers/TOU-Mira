using AmongUs.Data;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using TownOfUs.Modifiers;

namespace TownOfUs.Patches.Options;

[HarmonyPatch]
public static class TeamChatPatches
{
    public static bool SplitChats =>
        LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.SeparateChatBubbles.Value;
    public static GameObject TeamChatButton;
    private static TextMeshPro? _teamText;
    public static bool TeamChatActive; // True if any team chat is active
    public static int CurrentChatIndex = -1; // Index of currently selected chat (-1 = normal chat)
#pragma warning disable S2386
    public static GameObject? PrivateChatDot;
    public static SpriteRenderer PublicChatDot;
    public static Transform PublicChatItems;
    public static Transform PrivateChatItems;
    public static Transform MergedChatItems;
    public static Il2CppSystem.Collections.Generic.List<PoolableBehavior> PublicChatPool = new();
    public static Il2CppSystem.Collections.Generic.List<PoolableBehavior> PrivateChatPool = new();
    public static Il2CppSystem.Collections.Generic.List<PoolableBehavior> MergedChatPool = new();

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

    public static void CheckChatScrollers()
    {
        if (!PublicChatItems)
        {
            PublicChatItems = HudManager.Instance.Chat.scroller.Inner;
        }
        if (!PrivateChatItems)
        {
            PrivateChatItems = Object.Instantiate(PublicChatItems, PublicChatItems.parent);
            PrivateChatItems.name = "PrivateItems";
        }
        if (!MergedChatItems)
        {
            MergedChatItems = Object.Instantiate(PublicChatItems, PublicChatItems.parent);
            MergedChatItems.name = "MergedItems";
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

        var chat = HudManager.Instance.Chat;
        SoundManager.Instance.PlaySound(chat.quickChatButton.ClickSound, false, 1f, null);
        UpdateChat();

        if (!chat.IsOpenOrOpening)
        {
            chat.Toggle();
            UpdateChat();
        }
        else
        {
            // this reselects the text field
            chat.CheckKeyboardButton();
            chat.quickChatMenu.Clear();
            chat.quickChatMenu.Close();
            ConsoleJoystick.SetMode_QuickChat();
            ControllerManager.Instance.OpenOverlayMenu(chat.name, chat.backButton, chat.defaultButtonSelected, chat.controllerSelectable);
            if (Controller.currentTouchType != Controller.TouchType.Joystick)
            {
                chat.freeChatField.Focus();
            }
        }
    }

    public static void ForceNormalChat()
    {
        TeamChatActive = false;
        CurrentChatIndex = -1;

        if (HudManager.InstanceExists && HudManager.Instance.Chat != null)
        {
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

        CheckCurrentChats(chat);
        var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
        var Background = ChatScreenContainer?.transform.FindChild("Background");

        if (TeamChatActive)
        {
            // Ensure built-in chats are registered
            TeamChatManager.RegisterBuiltInChats();

            // Get currently selected chat
            var currentChat = TeamChatManager.GetCurrentChat();
            var availableChats = TeamChatManager.GetAllAvailableChats();

            // Set background color based on current chat's custom color, or use default
            if (Background != null)
            {
                var backgroundColor = currentChat?.BackgroundColor ?? new Color(0.2f, 0.1f, 0.1f, 0.8f);
                Background.GetComponent<SpriteRenderer>().color = backgroundColor;
            }

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
            if (PrivateChatDot != null)
            {
                var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                sprite.enabled = false;
            }
        }
        else
        {
            if (Background != null)
            {
                Background.GetComponent<SpriteRenderer>().color = Color.white;
            }
            HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouChatAssets.NormalChatIdle.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouChatAssets.NormalChatHover.LoadAsset();
            HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = TouChatAssets.NormalChatOpen.LoadAsset();
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
        PrivateChatDot.transform.localPosition -= new Vector3(0f, 0.425f, 0f);
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
            if (TeamChatActive)
            {
                UpdateChat();
            }

            if (PrivateChatDot != null &&
                (PlayerControl.LocalPlayer.IsLover() && MeetingHud.Instance == null || TeamChatActive))
            {
                var sprite = PrivateChatDot.GetComponent<SpriteRenderer>();
                sprite.enabled = false;
            }

            if (TeamChatButton)
            {
                return;
            }

            CreateTeamChatButton();
        }
    }

    public static void CheckCurrentChats(ChatController instance)
    {
        if (!LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.SeparateChatBubbles.Value)
        {
            PrivateChatItems.gameObject.SetActive(false);
            PublicChatItems.gameObject.SetActive(false);
            MergedChatItems.gameObject.SetActive(true);
            instance.scroller.Inner = MergedChatItems;
            instance.chatBubblePool.activeChildren = MergedChatPool;
            instance.scroller.SetYBoundsMin(MergedBoundsY);
        }
        else if (TeamChatActive)
        {
            MergedChatItems.gameObject.SetActive(false);
            PublicChatItems.gameObject.SetActive(false);
            PrivateChatItems.gameObject.SetActive(true);
            instance.scroller.Inner = PrivateChatItems;
            instance.chatBubblePool.activeChildren = PrivateChatPool;
            instance.scroller.SetYBoundsMin(PrivateBoundsY);
        }
        else
        {
            PublicChatItems.gameObject.SetActive(true);
            MergedChatItems.gameObject.SetActive(false);
            PrivateChatItems.gameObject.SetActive(false);
            instance.scroller.Inner = PublicChatItems;
            instance.chatBubblePool.activeChildren = PublicChatPool;
            instance.scroller.SetYBoundsMin(PublicBoundsY);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SendJailorChat)]
    public static void RpcSendJailorChat(PlayerControl player, string text)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
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
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
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
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
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
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
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
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
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
    private static void SetChatBubbleName(ChatBubble bubble, bool isDead, bool didVote, Color nameColor, string text)
    {
        bubble.SetName(text, isDead, didVote, nameColor);
    }

    public static float PublicBoundsY;
    public static float PrivateBoundsY;
    public static float MergedBoundsY;
    public static void AlignAllChatBubbles(ChatController instance)
    {
        float num = 0f;
        PublicChatItems.gameObject.SetActive(true);
        var activeChildren = PublicChatItems.GetComponentsInChildren<ChatBubble>();
        for (int i = activeChildren.Count - 1; i >= 0; i--)
        {
            var chatBubble = activeChildren[i].TryCast<ChatBubble>();
            num += chatBubble!.Background.size.y;
            Vector3 localPosition = chatBubble.transform.localPosition;
            localPosition.y = -1.85f + num;
            chatBubble.transform.localPosition = localPosition;
            num += 0.15f;
        }
        PublicBoundsY = Mathf.Min(0f, -num + instance.scroller.Hitbox.bounds.size.y + -0.3f);

        num = 0f;
        PrivateChatItems.gameObject.SetActive(true);
        activeChildren = PrivateChatItems.GetComponentsInChildren<ChatBubble>();
        for (int i = activeChildren.Count - 1; i >= 0; i--)
        {
            var chatBubble = activeChildren[i].TryCast<ChatBubble>();
            num += chatBubble!.Background.size.y;
            Vector3 localPosition = chatBubble.transform.localPosition;
            localPosition.y = -1.85f + num;
            chatBubble.transform.localPosition = localPosition;
            num += 0.15f;
        }
        PrivateBoundsY = Mathf.Min(0f, -num + instance.scroller.Hitbox.bounds.size.y + -0.3f);

        num = 0f;
        MergedChatItems.gameObject.SetActive(true);
        activeChildren = MergedChatItems.GetComponentsInChildren<ChatBubble>();
        for (int i = activeChildren.Count - 1; i >= 0; i--)
        {
            var chatBubble = activeChildren[i].TryCast<ChatBubble>();
            num += chatBubble!.Background.size.y;
            Vector3 localPosition = chatBubble.transform.localPosition;
            localPosition.y = -1.85f + num;
            chatBubble.transform.localPosition = localPosition;
            num += 0.15f;
        }
        MergedBoundsY = Mathf.Min(0f, -num + instance.scroller.Hitbox.bounds.size.y + -0.3f);

        var list = new Il2CppSystem.Collections.Generic.List<PoolableBehavior>();
        PublicChatItems.GetComponentsInChildren<PoolableBehavior>().Do(x => list.Add(x));
        PublicChatPool = list;

        list.Clear();
        PrivateChatItems.GetComponentsInChildren<PoolableBehavior>().Do(x => list.Add(x));
        PrivateChatPool = list;

        list.Clear();
        MergedChatItems.GetComponentsInChildren<PoolableBehavior>().Do(x => list.Add(x));
        MergedChatPool = list;

        if (!LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.SeparateChatBubbles.Value)
        {
            PrivateChatItems.gameObject.SetActive(false);
            PublicChatItems.gameObject.SetActive(false);
            MergedChatItems.gameObject.SetActive(true);
            instance.scroller.Inner = MergedChatItems;
            instance.chatBubblePool.activeChildren = MergedChatPool;
            instance.scroller.SetYBoundsMin(MergedBoundsY);
        }
        else if (TeamChatActive)
        {
            MergedChatItems.gameObject.SetActive(false);
            PublicChatItems.gameObject.SetActive(false);
            PrivateChatItems.gameObject.SetActive(true);
            instance.scroller.Inner = PrivateChatItems;
            instance.chatBubblePool.activeChildren = PrivateChatPool;
            instance.scroller.SetYBoundsMin(PrivateBoundsY);
        }
        else
        {
            PublicChatItems.gameObject.SetActive(true);
            MergedChatItems.gameObject.SetActive(false);
            PrivateChatItems.gameObject.SetActive(false);
            instance.scroller.Inner = PublicChatItems;
            instance.chatBubblePool.activeChildren = PublicChatPool;
            instance.scroller.SetYBoundsMin(PublicBoundsY);
        }
    }
    /*public static void ReclaimOldest(this ObjectPoolBehavior instance)
    {
        if (instance.activeChildren.Count > 0)
        {
            instance.Reclaim(instance.activeChildren[0]);
            return;
        }
        instance.InitPool(instance.Prefab);
    }
    public static ChatBubble GetChatBubble(this ObjectPoolBehavior instance)
    {
        List<PoolableBehavior> obj = instance.inactiveChildren;
        PoolableBehavior poolableBehavior;
        lock (obj)
        {
            if (instance.inactiveChildren.Count == 0)
            {
                if (instance.activeChildren.Count == 0)
                {
                    instance.InitPool(instance.Prefab);
                }
                else
                {
                    instance.CreateOneInactive(instance.Prefab);
                }
            }
            poolableBehavior = instance.inactiveChildren[instance.inactiveChildren.Count - 1];
            instance.inactiveChildren.RemoveAt(instance.inactiveChildren.Count - 1);
            instance.activeChildren.Add(poolableBehavior);
            PoolableBehavior poolableBehavior2 = poolableBehavior;
            int num = instance.childIndex;
            instance.childIndex = num + 1;
            poolableBehavior2.PoolIndex = num;
            if (instance.childIndex > instance.poolSize)
            {
                instance.childIndex = 0;
            }
        }
        if (instance.DetachOnGet)
        {
            poolableBehavior.transform.SetParent(null, false);
        }
        poolableBehavior.gameObject.SetActive(true);
        poolableBehavior.Reset();
        return poolableBehavior.Cast<ChatBubble>();
    }
    public static void Reclaim(this ObjectPoolBehavior instance, PoolableBehavior obj)
    {
        if (!instance)
        {
            DefaultPool.Instance.Reclaim(obj);
            return;
        }
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(instance.transform);
        var obj2 = instance.inactiveChildren;
        lock (obj2)
        {
            if (instance.activeChildren.Remove(obj))
            {
                instance.inactiveChildren.Add(obj);
            }
            else if (instance.inactiveChildren.Contains(obj))
            {
                Debug("ObjectPoolBehavior: :| Something was reclaimed without being gotten");
            }
            else
            {
                Debug("ObjectPoolBehavior: Destroying this thing I don't own");
                Object.Destroy(obj.gameObject);
            }
        }
    }*/


    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.GetPooledBubble))]
    public static bool GetPooledBubble(ChatController __instance, ref ChatBubble __result)
    {
        __result = __instance.chatBubblePool.Get<ChatBubble>();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
    public static bool AddChatNote(ChatController __instance, NetworkedPlayerInfo srcPlayer, ChatNoteTypes noteType)
    {
        if (srcPlayer == null)
        {
            return false;
        }
        ChatBubble pooledBubble = __instance.GetPooledBubble();
        var clonedBubble = __instance.GetPooledBubble();
        pooledBubble.SetCosmetics(srcPlayer);
        pooledBubble.transform.SetParent(PublicChatItems);
        pooledBubble.transform.localScale = Vector3.one;
        pooledBubble.SetNotification();
        clonedBubble.SetCosmetics(srcPlayer);
        clonedBubble.transform.SetParent(MergedChatItems);
        clonedBubble.transform.localScale = Vector3.one;
        clonedBubble.SetNotification();
        if (noteType == ChatNoteTypes.DidVote)
        {
            int rem = MeetingHud.Instance.GetVotesRemaining();
            var text = TranslationController.Instance.GetString(StringNames.MeetingHasVoted)
                .Replace("{0}", srcPlayer.PlayerName).Replace("{1}", rem.ToString(TownOfUsPlugin.Culture));
            SetChatBubbleName(pooledBubble, false, true, Color.green, text);
            SetChatBubbleName(clonedBubble, false, true, Color.green, text);
        }
        pooledBubble.SetText(string.Empty);
        clonedBubble.SetText(string.Empty);
        pooledBubble.AlignChildren();
        clonedBubble.AlignChildren();
        AlignAllChatBubbles(__instance);
        if (__instance is { IsOpenOrOpening: false })
        {
            __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
        }
        if (srcPlayer.Object != PlayerControl.LocalPlayer)
        {
            SoundManager.Instance.PlaySound(__instance.messageSound, false, 1f, null).pitch = 0.5f + (float)srcPlayer.PlayerId / 15f;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
	public static bool AddChat(ChatController __instance, PlayerControl sourcePlayer, string chatText, bool censor = true)
	{
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
        var clonedBubble = __instance.GetPooledBubble();
		try
		{
			pooledBubble.transform.SetParent(PublicChatItems);
            clonedBubble.transform.SetParent(MergedChatItems);
			pooledBubble.transform.localScale = Vector3.one;
            clonedBubble.transform.localScale = Vector3.one;
			bool flag = sourcePlayer.AmOwner;
			if (flag)
			{
				pooledBubble.SetRight();
                clonedBubble.SetRight();
			}
			else
			{
				pooledBubble.SetLeft();
                clonedBubble.SetLeft();
			}
			bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
			pooledBubble.SetCosmetics(data2);
            __instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
            clonedBubble.SetCosmetics(data2);
            __instance.SetChatBubbleName(clonedBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
			if (censor && DataManager.Settings.Multiplayer.CensorChat)
			{
				chatText = BlockedWords.CensorWords(chatText, false);
			}
			pooledBubble.SetText(chatText);
            clonedBubble.SetText(chatText);
			pooledBubble.AlignChildren();
            clonedBubble.AlignChildren();
            AlignAllChatBubbles(__instance);
            if (__instance is { IsOpenOrOpening: false })
            {
                __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
            }
			if (!flag && !__instance.IsOpenOrOpening)
			{
				SoundManager.Instance.PlaySound(__instance.messageSound, false, 1f, null).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
                __instance.chatNotification.SetUp(sourcePlayer, chatText);
			}
		}
		catch (Exception message)
		{
			ChatController.Logger.Error(message.ToString());
            __instance.chatBubblePool.Reclaim(pooledBubble);
            __instance.chatBubblePool.Reclaim(clonedBubble);
		}
        return false;
	}

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatWarning))]
	public static bool AddChatWarning(ChatController __instance, string warningText)
	{
		ChatBubble pooledBubble = __instance.GetPooledBubble();
        var clonedBubble = __instance.GetPooledBubble();
		try
		{
			pooledBubble.transform.SetParent(PublicChatItems);
            clonedBubble.transform.SetParent(MergedChatItems);
			pooledBubble.transform.localScale = Vector3.one;
            clonedBubble.transform.localScale = Vector3.one;
			pooledBubble.SetRight();
			pooledBubble.SetWarning(warningText);
            clonedBubble.SetRight();
            clonedBubble.SetWarning(warningText);
			pooledBubble.AlignChildren();
            clonedBubble.AlignChildren();
            AlignAllChatBubbles(__instance);
            if (__instance is { IsOpenOrOpening: false })
            {
                __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
            }
			SoundManager.Instance.PlaySound(__instance.warningSound, false, 1f, null);
		}
		catch (Exception message)
		{
			ChatController.Logger.Error(message.ToString());
            __instance.chatBubblePool.Reclaim(pooledBubble);
            __instance.chatBubblePool.Reclaim(clonedBubble);
		}
        return false;
	}

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static void SetUpScrollers(ChatController __instance)
    {
        CheckChatScrollers();
    }
}
