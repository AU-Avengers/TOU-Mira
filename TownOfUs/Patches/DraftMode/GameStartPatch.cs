using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime;
using MiraAPI.GameOptions;
using TownOfUs.Modules.DraftMode;
using TownOfUs.Options;
using TownOfUs.Roles.Other;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Patches.DraftMode
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public static class GameStartPatch
    {
        internal static bool SkipIntercept;

        [HarmonyPrefix]
        public static bool Prefix(GameStartManager __instance, out bool __state)
        {
            __state = true;

            if (!AmongUsClient.Instance.AmHost)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[GameStartPatch] Not host, allowing normal start");
                return true;
            }

            if (SkipIntercept)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[GameStartPatch] SkipIntercept enabled, allowing start");
                return true;
            }

            if (DraftManager.IsDraftActive)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[GameStartPatch] Draft already active, blocking start");
                __state = false;
                return false;
            }
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[GameStartPatch] RoleOptions not found");
                return true;
            }

            var distrib = roleOpts.CurrentRoleDistribution();
            if (distrib is not RoleDistribution.Draft)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[GameStartPatch] Not draft mode (distribution: {distrib}), allowing normal start");
                return true;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[GameStartPatch] DRAFT MODE DETECTED - Starting draft");

            var players = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => p != null && !p.Data.Disconnected && !SpectatorRole.TrackedSpectators.Contains(p.Data.PlayerName))
                .ToList();

            if (players.Count == 0)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, "[GameStartPatch] No players found, aborting draft");
                return true;
            }
            __state = false;

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[GameStartPatch] Starting draft with {players.Count} players");
            var shuffledSlots = Enumerable.Range(1, players.Count)
                .OrderBy(_ => Random.value)
                .ToList();
            var pidToSlot = new Dictionary<byte, int>();
            for (int i = 0; i < players.Count; i++)
                pidToSlot[players[i].PlayerId] = shuffledSlots[i];
            var engine = DraftEngineBehaviour.Instance;
            if (engine == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[GameStartPatch] Creating new DraftEngineBehaviour");
                var go = new GameObject("DraftEngineBehaviour");
                Object.DontDestroyOnLoad(go);
                engine = go.AddComponent(Il2CppType.From(typeof(DraftEngineBehaviour))).TryCast<DraftEngineBehaviour>()!;
            }

            if (engine == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, "[GameStartPatch] Failed to create DraftEngineBehaviour");
                return true;
            }

            __instance.countDownTimer = 10f;
            engine.StartHostDraft(players.Count, pidToSlot);
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[GameStartPatch] Draft started, blocking normal game start");
            return false;
        }

        [HarmonyPostfix]
        public static void Postfix(GameStartManager __instance, bool __state)
        {
            if (!__state)
            {
                return;
            }

            DraftScreenController.Hide();
            DraftStatusOverlay.SetState(OverlayState.Hidden);
            DraftCancelButton.Hide();

            if (SkipIntercept)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[GameStartPatch] Zeroing countDownTimer after BeginGame.");
                __instance.countDownTimer = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class DraftIntroCutsceneBeginPatch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            DraftScreenController.Hide();
            DraftStatusOverlay.SetState(OverlayState.Hidden);
            DraftCancelButton.Hide();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class DraftShipStatusStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftScreenController.Hide();
            DraftStatusOverlay.SetState(OverlayState.Hidden);
            DraftCancelButton.Hide();
        }
    }
}