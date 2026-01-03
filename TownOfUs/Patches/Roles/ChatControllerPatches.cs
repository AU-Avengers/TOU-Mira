using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Options;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(ChatController))]
public static class ChatControllerPatches
{
    private static TextMeshPro? _noticeText;
    public static List<string> ChatHistory { get; set; } = [];
    public static int CurrentHistorySelection = -1;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ChatController.Awake))]
    public static void AwakePostfix(ChatController __instance)
    {
        _noticeText =
            Object.Instantiate(__instance.sendRateMessageText, __instance.sendRateMessageText.transform.parent);
        _noticeText.text = string.Empty;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ChatController.UpdateChatMode))]
    public static void UpdateChatModePostfix(ChatController __instance)
    {
        if (_noticeText == null || !PlayerControl.LocalPlayer)
        {
            return;
        }

        if (!MeetingHud.Instance)
        {
            if (_noticeText.text != string.Empty)
            {
                _noticeText.text = string.Empty;
            }

            return;
        }

        if (PlayerControl.LocalPlayer.HasModifier<BlackmailedModifier>() &&
            !PlayerControl.LocalPlayer.Data.IsDead)
        {
            _noticeText.text = "You have been blackmailed.";
            __instance.freeChatField.SetVisible(false);
            __instance.quickChatField.SetVisible(false);
        }
        else if (TeamChatPatches.TeamChatActive && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            _noticeText.text = string.Empty;
            __instance.freeChatField.SetVisible(true);
            __instance.quickChatField.SetVisible(false);
        }
        else if (PlayerControl.LocalPlayer.HasModifier<JailedModifier>() &&
                 !PlayerControl.LocalPlayer.Data.IsDead && !TeamChatPatches.TeamChatActive)
        {
            var canChat = OptionGroupSingleton<JailorOptions>.Instance.JaileePublicChat;
            if (canChat)
            {
                _noticeText.text = "You are jailed. You can use public chat.";
                __instance.freeChatField.SetVisible(true);
            }
            else
            {
                _noticeText.text = "You are jailed. You cannot use public chat.";
                __instance.freeChatField.SetVisible(false);
                __instance.quickChatField.SetVisible(false);
            }
        }
        else
        {
            __instance.freeChatField.SetVisible(true);
            _noticeText.text = string.Empty;
        }
    }

    //  Allow all symbols
    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
    public static class IsCharAllowedPatch
    {
        public static bool Prefix(char i, ref bool __result)
        {
            // Error($"checking if character {i.ToString()} (id {(int)i}) is allowed in chat");
            if (i is '\b' or '\n' or '\r' or '>')
            {   
                return true;
            }

            // Allow all printable Unicode
            __result = true;
            return false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Start))]
    public static void TextBoxPostfix(TextBoxTMP __instance)
    {
        __instance.allowAllCharacters = true;
        __instance.AllowEmail = true;
        __instance.AllowSymbols = true;
    }


    //  Char limit and clipboard shortcuts
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ChatController.Update))]
    public static void UpdatePostfix(ChatController __instance)
    {
        var field = __instance.freeChatField?.textArea;
        if (field == null) return;

        // Bigger character limit
        field.characterLimit = 300;

        __instance?.freeChatField?.UpdateCharCount();

        // CTRL + C
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.C))
        {
            GUIUtility.systemCopyBuffer = field.text;
        }

        // CTRL + X
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.X))
        {
            GUIUtility.systemCopyBuffer = field.text;
            field.SetText("");
        }

        // Up Arrow - Previous chat history
        if (Input.GetKeyDown(KeyCode.UpArrow) && ChatHistory.Count > 0)
        {
            CurrentHistorySelection = Mathf.Clamp(--CurrentHistorySelection, 0, ChatHistory.Count - 1);
            __instance?.freeChatField?.textArea.SetText(ChatHistory[CurrentHistorySelection]);
        }

        // Down Arrow - Next chat history
        if (Input.GetKeyDown(KeyCode.DownArrow) && ChatHistory.Count > 0)
        {
            CurrentHistorySelection++;
            __instance?.freeChatField?.textArea.SetText(CurrentHistorySelection < ChatHistory.Count ? ChatHistory[CurrentHistorySelection] : string.Empty);
        }
    }
}