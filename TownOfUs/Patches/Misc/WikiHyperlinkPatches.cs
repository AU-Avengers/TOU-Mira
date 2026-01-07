using System.Text;
using System.Text.RegularExpressions;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;
using TMPro;
using TownOfUs.Modules.Components;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

public static class WikiHyperLinkPatches
{
    private static string fontTag =
        "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - BlackOutlineMasked\">";

    public static string
        CheckForTags(string text, TextMeshPro tmp) // In theory, this method can be used for any TMP object
    {
        var roleTags = Regex.Matches(text, @"#\w+(-\w+)*");
        var modifierTags = Regex.Matches(text, @"&\w+(-\w+)*");

        if (roleTags.Count <= 0 && modifierTags.Count <= 0)
        {
            return text;
        }

        tmp.outlineColor = Color.black;
        var ogOutline = tmp.outlineWidth;
        tmp.outlineWidth = 0.15f;

        int lastIndex = 0;
        var sb = new StringBuilder();

        int linkIndex = 0;
        // We get all tags and order them by index in the string, so link indexes are not messed up
        foreach (Match match in roleTags.Union(modifierTags).OrderBy(x => x.Index))
        {
            int count = match.Index - lastIndex;
            if (count > 0)
            {
                sb.Append(text, lastIndex, count);
            }

            string key = match.Value.Substring(1).Replace('-', ' ');
            string replacement = match.Value;
            bool shouldHyperlink = true;
            if (match.Value[0] == '#') // Role tag
            {
                var role = MiscUtils.AllRegisteredRoles.FirstOrDefault(x =>
                    x.GetRoleName().Equals(key, StringComparison.OrdinalIgnoreCase));
                if (role is ICustomRole customRole)
                {
                    replacement =
                        $"{fontTag}<b>{customRole.RoleColor.ToTextColor()}<link={customRole.GetType().FullName}:{linkIndex}>{customRole.RoleName}</link></color></b></font>";
                    shouldHyperlink = customRole is IWikiDiscoverable || SoftWikiEntries.RoleEntries.ContainsKey(role);
                }
                else if (role != null && SoftWikiEntries.RoleEntries.ContainsKey(role))
                {
                    replacement =
                        $"{fontTag}<b>{role.TeamColor.ToTextColor()}<link={role.GetType().FullName}:{linkIndex}>{role.GetRoleName()}</link></color></b></font>";
                    if (Enum.IsDefined(role.Role))
                    {
                        replacement =
                        $"{fontTag}<b>{role.TeamColor.ToTextColor()}<link={$"AmongUs.Roles.{role.Role.ToString()}"}:{linkIndex}>{role.GetRoleName()}</link></color></b></font>";
                    }
                    shouldHyperlink = true;
                }
                else
                {
                    // Some non-custom roles (specifically Impostor and Crewmate) can also be tagged, but they have no wiki entries.
                    role = MiscUtils.AllRegisteredRoles.FirstOrDefault(x =>
                        x.GetRoleName().Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (role != null)
                    {
                        if (role.Role is RoleTypes.Crewmate || role.Role is RoleTypes.Impostor)
                        {
                            replacement =
                                $"{fontTag}<b>{role.TeamColor.ToTextColor()}{role.GetRoleName()}</color></b></font>";
                            shouldHyperlink = false;
                        }
                        else
                        {
                            replacement =
                                $"{fontTag}<b>{role.TeamColor.ToTextColor()}<link={$"AmongUs.Roles.{role.Role.ToString()}"}:{linkIndex}>{role.GetRoleName()}</link></color></b></font>";
                            shouldHyperlink = true;
                        }
                    }
                }
            }
            else if (match.Value[0] == '&') // Modifier tag
            {
                var modifier = MiscUtils.AllModifiers
                    .Where(m => m is GameModifier)
                    .FirstOrDefault(x => x.ModifierName.Equals(key, StringComparison.OrdinalIgnoreCase));

                if (modifier != null)
                {
                    replacement =
                        $"{fontTag}<b>{modifier.FreeplayFileColor.ToTextColor()}<link={modifier.GetType().FullName}:{linkIndex}>{modifier.ModifierName}</link></color></b></font>";
                    shouldHyperlink = modifier is IWikiDiscoverable;
                }
            }

            sb.Append(replacement);

            lastIndex = match.Index + match.Length;

            if (shouldHyperlink)
            {
                // The hyperlink knows where it is by knowing where it isn't
                var hyperlink = tmp.gameObject.AddComponent<WikiHyperlink>();
                hyperlink.HyperlinkIndex = linkIndex;
                hyperlink.HyperlinkString = replacement;
                hyperlink.HoverHyperlinkString = $"<i>{replacement}</i>";
                linkIndex++;
            }
        }

        sb.Append(text, lastIndex, text.Length - lastIndex);
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