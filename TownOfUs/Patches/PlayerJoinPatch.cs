using System.Collections;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class PlayerJoinPatch
{
    public static bool SentOnce { get; private set; }
    public static HudManager HUD => HudManager.Instance;

    internal static void Postfix()
    {
        Coroutines.Start(CoSendJoinMsg());
    }

    internal static IEnumerator CoSendJoinMsg()
    {
        while (!AmongUsClient.Instance)
        {
            yield return null;
        }

        Info("Client Initialized?");

        while (!PlayerControl.LocalPlayer)
        {
            yield return null;
        }

        var player = PlayerControl.LocalPlayer;

        while (!player)
        {
            yield return null;
        }

        if (!player.AmOwner)
        {
            yield break;
        }

        if (!SentOnce)
        {
            var mods = IL2CPPChainloader.Instance.Plugins;
            var modDictionary = new Dictionary<byte, string>();
            byte modByte = 0;
            foreach (var mod in mods)
            {
                modDictionary.Add(modByte, $"{mod.Value.Metadata.Name}: {mod.Value.Metadata.Version}");
                modByte++;
            }

            Rpc<SendClientModInfoRpc>.Instance.Send(PlayerControl.LocalPlayer, modDictionary);
        }

        Info("Sending Message to Local Player...");
        TouRoleManagerPatches.ReplaceRoleManager = false;
        SpectatorRole.TrackedPlayers.Clear();
        SpectatorRole.FixedCam = false;

        var time = 0f;
        if (GameHistory.EndGameSummary != string.Empty && LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance
                .ShowSummaryMessageToggle.Value)
        {
            var factionText = string.Empty;
            var msg = string.Empty;
            if (GameHistory.WinningFaction != string.Empty)
            {
                factionText = $"<size=80%>Winning Team: {GameHistory.WinningFaction}</size>\n";
            }

            var title =
                $"<color=#8BFDFD>System (Toggleable In Options)</color>\n<size=62%>{factionText}{GameHistory.EndGameSummary}</size>";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
        }

        if (!SentOnce && LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowWelcomeMessageToggle.Value)
        {
            var name = "<color=#8BFDFD>System</color>";
            var msg =
                $"Welcome to Town of Us Mira v{TownOfUsPlugin.Version}!\nUse the wiki (the globe icon) to get more info on roles or modifiers, where you can use the searchbar. Otherwise use /help in the chat to get a list of commands.\nYou can also disable this message through your options menu.";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, name, msg, true);
            time = 5f;
        }
        else if (!LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowWelcomeMessageToggle.Value)
        {
            time = 2.48f;
        }

        if (time == 0)
        {
            yield break;
        }

        yield return new WaitForSeconds(time);
        Info("Offset Wiki Button (if needed)");
        SentOnce = true;
    }
}