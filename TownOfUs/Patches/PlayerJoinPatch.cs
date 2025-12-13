using System.Collections;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class PlayerJoinPatch
{
    public static bool SentOnce { get; private set; }
    public static HudManager HUD => HudManager.Instance;

    public static void Zoom(bool zoomOut)
    {
        if (((PlayerControl.LocalPlayer.DiedOtherRound() &&
              (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } ||
               PlayerControl.LocalPlayer.Data.Role is not IGhostRole)) || TutorialManager.InstanceExists)
            && !MeetingHud.Instance && Minigame.Instance == null &&
            !HudManager.Instance.Chat.IsOpenOrOpening)
        {
            HudManagerPatches.ScrollZoom(zoomOut);
        }
    }

    internal static void Postfix()
    {
        TouKeybinds.ZoomIn.OnActivate(() =>
        {
            Zoom(false);
        });
        TouKeybinds.ZoomInKeypad.OnActivate(() =>
        {
            Zoom(false);
        });
        TouKeybinds.ZoomOut.OnActivate(() =>
        {
            Zoom(true);
        });
        TouKeybinds.ZoomOutKeypad.OnActivate(() =>
        {
            Zoom(true);
        });
        TouKeybinds.Wiki.OnActivate(() =>
        {
            if (Minigame.Instance)
            {
                return;
            }

            IngameWikiMinigame.Create().Begin(null);
        });
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
        var systemName = $"<color=#8BFDFD>{TouLocale.Get("SystemChatTitle")}</color>";

        var time = 0f;
        var summary = GameHistory.EndGameSummary;
        switch (LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.SummaryMessageAppearance.Value)
        {
            case GameSummaryAppearance.Advanced:
                summary = GameHistory.EndGameSummaryAdvanced;
                break;
            case GameSummaryAppearance.Simplified:
                summary = GameHistory.EndGameSummarySimple;
                break;
        }
        if (summary != string.Empty && LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance
                .ShowSummaryMessageToggle.Value)
        {
            systemName = $"<color=#8BFDFD>{TouLocale.Get("EndGameSummary")}</color>";
            var factionText = string.Empty;
            var msg = string.Empty;
            if (GameHistory.WinningFaction != string.Empty)
            {
                factionText = $"<size=80%>{TouLocale.GetParsed("EndResult").Replace("<victoryType>", GameHistory.WinningFaction)}</size>\n";
            }

            var title =
                $"{systemName}\n<size=62%>{factionText}{summary}</size>";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
        }

        if (!SentOnce && LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowWelcomeMessageToggle.Value)
        {
            var msg = TouLocale.GetParsed("WelcomeMessageBlurb").Replace("<modVersion>", TownOfUsPlugin.Version);
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName, msg, true);
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