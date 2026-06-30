using System.Collections;
using System.Text.RegularExpressions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class AntiRichTextNamePatch
{
    private static readonly Regex RichTextPattern = new Regex(
        @"<[^>]+>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly HashSet<byte> _warnedPlayers = [];

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckName))]
    [HarmonyPostfix]
    public static void CheckNamePostfix(PlayerControl __instance)
    {
        string name = __instance.Data?.PlayerName ?? "";
        if (!RichTextPattern.IsMatch(name)) return;

        if (__instance == PlayerControl.LocalPlayer)
        {
            if (_warnedPlayers.Contains(__instance.PlayerId)) return;
            _warnedPlayers.Add(__instance.PlayerId);

            Helpers.CreateAndShowNotification(
                "Warning: your name contains rich text tags and may get you kicked.",
                Color.yellow
            );
            return;
        }
        if (!AmongUsClient.Instance.AmHost) return;
        if (!OptionGroupSingleton<HostSpecificOptions>.Instance.KickRichTextNames) return;

        Helpers.CreateAndShowNotification(
            $"<b>{name}</b> was kicked for having Unity Rich Text tags in name.",
            Color.red
        );

        Coroutines.Start(KickAfterDelay(__instance));
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    [HarmonyPostfix]
    public static void ExitGame_Postfix()
    {
        _warnedPlayers.Clear();
    }

    private static IEnumerator KickAfterDelay(PlayerControl player)
    {
        yield return new WaitForSeconds(1.5f);
        AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
    }
}