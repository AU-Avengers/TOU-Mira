using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Modules.DraftMode;
using TownOfUs.Options;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Patches.DraftMode
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public static class GameStartPatch
    {
        internal static bool SkipIntercept;
        internal static bool PostDraftCountdownActive;

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
                if (PostDraftCountdownActive)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                        "[GameStartPatch] Suppressing native BeginGame; post-draft countdown owns completion");
                    __state = false;
                    return false;
                }

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
                .Where(p => p != null && !p.Data.Disconnected && !SpectatorRole.TrackedSpectators.Contains(p.GetBestPlayerDenomination()))
                .ToList();

            if (players.Count == 0)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, "[GameStartPatch] No players found, aborting draft");
                return true;
            }
            __state = false;
            __instance.countDownTimer = 10f;
            DraftEngine.Start(players);
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