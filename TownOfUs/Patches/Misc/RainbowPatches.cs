using AmongUs.Data;
using HarmonyLib;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Utilities;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace TownOfUs.RainbowMod;

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
public static class SetPlayerMaterialPatch
{
    public static bool Prefix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer rend)
    {
        var r = rend.gameObject.GetComponent<RainbowBehaviour>();
        if (r == null)
        {
            r = rend.gameObject.AddComponent<RainbowBehaviour>();
        }

        r.AddRend(rend, colorId);
        return !RainbowUtils.IsRainbow(colorId);
    }
}

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(Color), typeof(Renderer))]
public static class SetPlayerMaterialPatch2
{
    public static bool Prefix([HarmonyArgument(1)] Renderer rend)
    {
        var r = rend.gameObject.GetComponent<RainbowBehaviour>();
        if (r == null)
        {
            r = rend.gameObject.AddComponent<RainbowBehaviour>();
        }

        r.AddRend(rend, 0);
        return true;
    }
}

[HarmonyPatch(typeof(PlayerTab))]
public static class PlayerTabPatch
{
    [HarmonyPatch(nameof(PlayerTab.Update))]
    [HarmonyPostfix]
    public static void UpdatePostfix(PlayerTab __instance)
    {
        for (var i = 0; i < __instance.ColorChips.Count; i++)
        {
            if (RainbowUtils.IsRainbow(i))
            {
                __instance.ColorChips[i].Inner.SpriteColor = RainbowUtils.Rainbow;
                break;
            }
        }
    }
}

[HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.Update))]
public static class ChatNotifRainbowPatch
{
    public static void Prefix(ChatNotification __instance)
    {
        if (__instance.gameObject.active && RainbowUtils.IsRainbow(__instance.player.cosmetics.ColorId))
        {
            string str = ColorUtility.ToHtmlStringRGB(RainbowUtils.SetBasicRainbow());
            __instance.playerNameText.text = "<color=#" + str + ">" + __instance.playerNameText.text.WithoutRichText();
        }
    }

    public static void Postfix(ChatNotification __instance)
    {
        if (__instance.gameObject.active && RainbowUtils.IsRainbow(__instance.player.cosmetics.ColorId))
        {
            string str = ColorUtility.ToHtmlStringRGB(RainbowUtils.SetBasicRainbow());
            __instance.playerNameText.text = "<color=#" + str + ">" + __instance.playerNameText.text.WithoutRichText();
        }
    }
}

[HarmonyPatch(typeof(HostInfoPanel), nameof(HostInfoPanel.Update))]
public static class RainbowLobbyInfoPanePatch
{
    public static void Prefix(HostInfoPanel __instance)
    {
        if (__instance.gameObject.activeInHierarchy && RainbowUtils.IsRainbow(__instance.player.cosmetics.ColorId))
        {
            NetworkedPlayerInfo host = GameData.Instance.GetHost();
            string text = ColorUtility.ToHtmlStringRGB(RainbowUtils.SetBasicRainbow());
            __instance.hostLabel.text =
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HostNounLabel,
                    Array.Empty<Object>());
            if (__instance.ShouldBoldenHostLabel(DataManager.Settings.Language.CurrentLanguage))
            {
                __instance.hostLabel.text = __instance.hostLabel.text.Insert(0, "<b>");
                __instance.hostLabel.text = __instance.hostLabel.text.Insert(__instance.hostLabel.text.Length, "</b>");
            }

            if (AmongUsClient.Instance.AmHost)
            {
                __instance.playerName.text = (string.IsNullOrEmpty(host.PlayerName)
                                                 ? "..."
                                                 : $"<color=#{text}>{host.PlayerName}</color>")
                                             + "  <size=90%><b><font=\"Barlow-BoldItalic SDF\" material=\"Barlow-BoldItalic SDF Outline\">" +
                                             DestroyableSingleton<TranslationController>.Instance.GetString(
                                                 StringNames.HostYouLabel, Array.Empty<Object>());
            }
            else
            {
                __instance.playerName.text =
                    (string.IsNullOrEmpty(host.PlayerName) ? "..." : $"<color=#{text}>{host.PlayerName}</color>") +
                    " (" + __instance.player.ColorBlindName + ")";
            }
        }
    }

    public static void Postfix(HostInfoPanel __instance)
    {
        if (__instance.gameObject.activeInHierarchy && RainbowUtils.IsRainbow(__instance.player.cosmetics.ColorId))
        {
            NetworkedPlayerInfo host = GameData.Instance.GetHost();
            string text = ColorUtility.ToHtmlStringRGB(RainbowUtils.SetBasicRainbow());
            __instance.hostLabel.text =
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HostNounLabel,
                    Array.Empty<Object>());
            if (__instance.ShouldBoldenHostLabel(DataManager.Settings.Language.CurrentLanguage))
            {
                __instance.hostLabel.text = __instance.hostLabel.text.Insert(0, "<b>");
                __instance.hostLabel.text = __instance.hostLabel.text.Insert(__instance.hostLabel.text.Length, "</b>");
            }

            if (AmongUsClient.Instance.AmHost)
            {
                __instance.playerName.text = (string.IsNullOrEmpty(host.PlayerName)
                                                 ? "..."
                                                 : $"<color=#{text}>{host.PlayerName}</color>")
                                             + "  <size=90%><b><font=\"Barlow-BoldItalic SDF\" material=\"Barlow-BoldItalic SDF Outline\">" +
                                             DestroyableSingleton<TranslationController>.Instance.GetString(
                                                 StringNames.HostYouLabel, Array.Empty<Object>());
            }
            else
            {
                __instance.playerName.text =
                    (string.IsNullOrEmpty(host.PlayerName) ? "..." : $"<color=#{text}>{host.PlayerName}</color>") +
                    " (" + __instance.player.ColorBlindName + ")";
            }
        }
    }
}