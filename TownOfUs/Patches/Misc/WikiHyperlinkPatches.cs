using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using UnityEngine;
using TMPro;
using TownOfUs.Modules.Components;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

public static class WikiHyperLinkPatches
{
    private static string fontTag = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - BlackOutlineMasked\">";
    
    public static string CheckForTags(string chatText, TextMeshPro tmp)
    {
        var roleTags = Regex.Matches(chatText, @"#\w+(-\w+)*");
        var modifierTags = Regex.Matches(chatText, @"&\w+(-\w+)*");

        if (roleTags.Count <= 0 && modifierTags.Count <= 0)
        {
            return chatText;
        }
            
        tmp.gameObject.AddComponent<WikiHyperlink>();
        
        tmp.outlineColor = Color.black;
        var ogOutline = tmp.outlineWidth;
        tmp.outlineWidth = 0.15f;
            
        int lastIndex = 0;
        var sb = new StringBuilder();

        foreach (Match match in roleTags)
        {
            int count = match.Index - lastIndex;
            if (count > 0)
                sb.Append(chatText, lastIndex, count);

            string roleName = match.Value.Substring(1).Replace('-', ' ');
            var role = MiscUtils.AllRoles.FirstOrDefault(x => x.NiceName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                
            string replacement = match.Value;
            if (role is ICustomRole customRole)
            {
                replacement = $"{fontTag}<b>{customRole.RoleColor.ToTextColor()}<link={customRole.GetType().FullName}>{customRole.RoleName}</link></color></b></font>";
            }
            else
            {
                // Non-custom roles (aka vanilla ones) can also be tagged, but they have no wiki entries.
                role = RoleManager.Instance.AllRoles.FirstOrDefault(x => x.NiceName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (role != null)
                {
                    replacement = $"{fontTag}<b>{role.TeamColor.ToTextColor()}<link={role.NiceName}Role>{role.NiceName}</link></color></b></font>";
                }
            }
                
            sb.Append(replacement);

            lastIndex = match.Index + match.Length;
        }

        foreach (Match match in modifierTags)
        {
            int count = match.Index - lastIndex;
            if (count > 0)
                sb.Append(chatText, lastIndex, count);
                
            string modifierName = match.Value.Substring(1).Replace('-', ' ');
            var modifier = MiscUtils.AllModifiers
                .Where(m => m is GameModifier)
                .FirstOrDefault(x => x.ModifierName.Equals(modifierName, StringComparison.OrdinalIgnoreCase));

            string replacement = match.Value;
            if (modifier != null) 
            { 
                replacement = $"{fontTag}<b>{modifier.FreeplayFileColor.ToTextColor()}<link={modifier.GetType().FullName}>{modifier.ModifierName}</link></color></b></font>";
            }
                
            sb.Append(replacement);
            lastIndex = match.Index + match.Length;
        }

        sb.Append(chatText, lastIndex, chatText.Length - lastIndex);

        tmp.outlineWidth = ogOutline;
        
        return sb.ToString();
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
    public static class ChatBubble_SetText
    {
        public static void Prefix(ChatBubble __instance, ref string chatText)
        {
            chatText = CheckForTags(chatText, __instance.TextArea);
        }
    }
    
    [HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.SetUp))]
    public static class ChatNotification_SetUp
    {
        public static void Prefix(ChatNotification __instance, ref string text)
        {
            text = CheckForTags(text, __instance.chatText);
        }
    }
}