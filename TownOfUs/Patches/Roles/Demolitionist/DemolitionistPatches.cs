using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using MiraAPI.Hud;
using TownOfUs.Buttons.Neutral.NeutralEvil;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral.NeutralEvil;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class DemolitionistPatches
{
    public static void Register(ManualLogSource log) => SabotageMap.Register(log);

    #region Emergency / imp sabotage mutex / lifecycle

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
    [HarmonyPostfix]
    public static void EmergencyMinigameBeginPostfix(EmergencyMinigame __instance)
    {
        if (!DemolitionistSabotageState.IsActive)
        {
            return;
        }

        ApplySabotageEmergencyDisabledUi(__instance);
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    [HarmonyPostfix]
    public static void EmergencyMinigameUpdatePostfix(EmergencyMinigame __instance)
    {
        if (!DemolitionistSabotageState.IsActive)
        {
            return;
        }

        ApplySabotageEmergencyDisabledUi(__instance);
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    [HarmonyPostfix]
    public static void EmergencyConsoleCanUsePostfix(
        SystemConsole __instance,
        NetworkedPlayerInfo pc,
        ref bool canUse,
        ref bool couldUse)
    {
        if (!DemolitionistSabotageState.IsActive || !IsEmergencyConsole(__instance) || pc?.Object == null)
        {
            return;
        }

        if (!IsWithinEmergencyUseDistance(__instance, pc.Object))
        {
            return;
        }

        couldUse = true;
        canUse = false;
    }

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool SabotageButtonDoClickPrefix() => !DemolitionistSabotageState.IsActive;

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEndPostfix()
    {
        DemolitionistSabotageState.ResetAll(clearProgress: false);
        DemolitionistUtilityConsoles.InvalidateCache();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        DemolitionistSabotageState.ResetAll();
        DemolitionistUtilityConsoles.InvalidateCache();
    }

    private static void ApplySabotageEmergencyDisabledUi(EmergencyMinigame minigame)
    {
        minigame.StatusText.text = GetEmergencyDuringSabotageText();
        minigame.NumberText.text = string.Empty;
        minigame.ClosedLid.gameObject.SetActive(true);
        minigame.OpenLid.gameObject.SetActive(false);
        minigame.ButtonActive = false;
    }

    private static string GetEmergencyDuringSabotageText()
    {
        try
        {
            return TranslationController.Instance.GetString(StringNames.EmergencyDuringCrisis);
        }
        catch
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Notification.EmergencyDuringSabotage"); //just in case
        }
    }

    private static bool IsEmergencyConsole(SystemConsole console)
    {
        return console?.MinigamePrefab != null
            && console.MinigamePrefab.TryCast<EmergencyMinigame>() != null;
    }

    private static bool IsWithinEmergencyUseDistance(SystemConsole console, PlayerControl player)
    {
        var dist = Vector2.Distance(player.GetTruePosition(), (Vector2)console.transform.position);
        return dist <= DemolitionistUtilityConsoles.GetUsableDistance(console);
    }

    #endregion

    #region Sabotage map overlay

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal static class ButtonVisibility
    {
        // Plant/defuse button visibility is handled by the buttons themselves (TownOfUsButton
        // slaves the gameObject to the vanilla HUD buttons, so map/lobby/meeting are covered).
        [HarmonyPriority(Priority.Last)]
        private static void Postfix()
        {
            SabotageMap.HudManagerUpdatePostfix();
        }
    }

    #endregion

    #region Blown-up utility consoles

    [HarmonyPatch]
    internal static class ConsoleDisable
    {
        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
        [HarmonyPrefix]
        public static bool MapConsoleCanUsePrefix(
            MapConsole __instance,
            NetworkedPlayerInfo pc,
            ref bool canUse,
            ref bool couldUse,
            ref float __result)
        {
            if (__instance == null || pc == null)
            {
                return true;
            }

            var pos = (Vector2)__instance.transform.position;
            var key = DemolitionistUtilityConsoles.GetStableId(DemolitionistUtilityKind.Admin, pos);
            return ApplyDisable(pc, key, DemolitionistUtilityKind.Admin, ref canUse, ref couldUse, ref __result);
        }

        [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
        [HarmonyPrefix]
        public static bool SystemConsoleCanUsePrefix(
            SystemConsole __instance,
            NetworkedPlayerInfo pc,
            ref bool canUse,
            ref bool couldUse,
            ref float __result)
        {
            if (__instance == null || pc == null)
            {
                return true;
            }

            if (!DemolitionistUtilityConsoles.TryClassifySystemConsole(__instance, out var kind))
            {
                return true;
            }

            var pos = (Vector2)__instance.transform.position;
            var key = DemolitionistUtilityConsoles.GetStableId(kind, pos);
            return ApplyDisable(pc, key, kind, ref canUse, ref couldUse, ref __result);
        }

        private static bool ApplyDisable(
            NetworkedPlayerInfo pc,
            int consoleKey,
            DemolitionistUtilityKind kind,
            ref bool canUse,
            ref bool couldUse,
            ref float __result)
        {
            if (!OptionGroupSingleton<DemolitionistOptions>.Instance.DisableExplodedConsoles)
            {
                return true;
            }

            if (!DemolitionistSabotageState.IsUtilityDisabled(consoleKey, kind))
            {
                return true;
            }

            canUse = false;
            couldUse = false;
            __result = float.MaxValue;
            return false;
        }
    }

    #endregion

    #region Imp sabotage map (grey/disable while Demolitionist sabo active)

    internal static class SabotageMap
    {
        private static readonly Color DisabledTint = new(0.45f, 0.45f, 0.45f, 0.55f);
        private static readonly Color RestoreTint = Color.white;

        private static bool _sabotageMapOpen;

        public static void Register(ManualLogSource log)
        {
            log.LogInfo("Demolitionist sabotage map: using MapBehaviour / InfectedOverlay patches.");
        }

        public static void HudManagerUpdatePostfix()
        {
            if (!_sabotageMapOpen)
            {
                return;
            }

            var map = MapBehaviour.Instance;
            if (map == null || !map.IsOpen)
            {
                return;
            }

            ApplyState(map, DemolitionistSabotageState.IsActive);
        }

        public static void OnMapShown(MapBehaviour map, MapOptions options)
        {
            _sabotageMapOpen = options.Mode == MapOptions.Modes.Sabotage;
            if (!_sabotageMapOpen)
            {
                return;
            }

            ApplyState(map, DemolitionistSabotageState.IsActive);
        }

        public static void OnSabotageMapShown(MapBehaviour map)
        {
            _sabotageMapOpen = true;
            ApplyState(map, DemolitionistSabotageState.IsActive);
        }

        public static void OnMapClosed()
        {
            _sabotageMapOpen = false;
        }

        public static bool AllowMapRoomSabotage() => !DemolitionistSabotageState.IsActive;

        private static void ApplyState(MapBehaviour map, bool blocked)
        {
            var overlay = map.infectedOverlay;
            if (overlay == null)
            {
                return;
            }

            var tint = blocked ? DisabledTint : RestoreTint;
            var interactive = !blocked;

            if (overlay.allButtons != null)
            {
                foreach (var btn in overlay.allButtons)
                {
                    if (IsDoorButton(btn))
                    {
                        SetButtonState(btn, RestoreTint, true);
                        continue;
                    }

                    SetButtonState(btn, tint, interactive);
                }
            }

            if (overlay.rooms == null)
            {
                return;
            }

            foreach (var room in overlay.rooms)
            {
                if (room == null)
                {
                    continue;
                }

                // Doors always usable + white.
                SetSpriteState(room.door, RestoreTint, true);
                // Room sabotage icon: grey + disabled when demolitionist sabo active.
                SetSpriteState(room.special, tint, interactive);
            }
        }

        private static bool IsDoorButton(ButtonBehavior? btn)
        {
            if (btn == null || btn.gameObject == null)
            {
                return false;
            }

            var name = btn.gameObject.name;
            return !string.IsNullOrEmpty(name)
                && name.Contains("door", System.StringComparison.OrdinalIgnoreCase);
        }

        private static void SetButtonState(ButtonBehavior? behavior, Color color, bool enabled) =>
            ApplyComponentState(behavior, behavior?.spriteRenderer, color, enabled);

        private static void SetSpriteState(SpriteRenderer? sprite, Color color, bool enabled) =>
            ApplyComponentState(sprite, sprite, color, enabled);

        private static void ApplyComponentState(Component? root, SpriteRenderer? sprite, Color color, bool enabled)
        {
            if (root == null)
            {
                return;
            }

            if (sprite != null)
            {
                sprite.color = color;
            }

            var collider = root.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = enabled;
            }

            var passive = root.GetComponent<PassiveButton>();
            if (passive != null)
            {
                passive.enabled = enabled;
            }

            var btn = root as ButtonBehavior ?? root.GetComponent<ButtonBehavior>();
            if (btn != null)
            {
                btn.enabled = enabled;
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show), typeof(MapOptions))]
    internal static class DemolitionistMapBehaviourShowPatch
    {
        [HarmonyPostfix]
        private static void Postfix(MapBehaviour __instance, MapOptions opts) =>
            SabotageMap.OnMapShown(__instance, opts);
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    internal static class DemolitionistMapBehaviourShowSabotageMapPatch
    {
        [HarmonyPostfix]
        private static void Postfix(MapBehaviour __instance) =>
            SabotageMap.OnSabotageMapShown(__instance);
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
    internal static class DemolitionistMapBehaviourClosePatch
    {
        [HarmonyPostfix]
        private static void Postfix() => SabotageMap.OnMapClosed();
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageHeli))]
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageMushroomMixup))]
    internal static class DemolitionistMapRoomSabotageBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix() => SabotageMap.AllowMapRoomSabotage();
    }

    #endregion
}
