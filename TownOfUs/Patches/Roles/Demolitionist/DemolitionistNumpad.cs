using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Text;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Options;
using TownOfUs.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches;

internal enum DemolitionistNumpadAction
{
    None,
    Plant,
    Defuse,
}

internal static class DemolitionistNumpad
{
    public static void Register(Harmony harmony, ManualLogSource log) => Keypad.Register(harmony, log);

    #region Session controller

    internal static class Controller
    {
        private static DemolitionistKeypadNoOxyTask? _task;
        private static Minigame? _minigame;
        private static DemolitionistNumpadAction _action;
        private static Vector2 _plantPosition;
        private static int _plantConsoleKey;
        private static DemolitionistUtilityKind _plantKind;
        private static bool _numpadSessionCancelled;
        private static int _openNumpadSessionId;
        private static bool _plantSucceeded;
        public static bool InProgress => _task != null;
        public static bool DefuseInProgress => _task != null && _action == DemolitionistNumpadAction.Defuse;

        public static bool ConsumePlantSuccess()
        {
            if (!_plantSucceeded)
            {
                return false;
            }

            _plantSucceeded = false;
            return true;
        }

        internal static int ActiveOpenNumpadSessionId => _openNumpadSessionId;

        public static void ResetAll()
        {
            CleanupTaskAndMinigame();
            _numpadSessionCancelled = false;
            _plantSucceeded = false;
            _action = DemolitionistNumpadAction.None;
            _plantPosition = Vector2.zero;
            _plantConsoleKey = 0;
            _plantKind = DemolitionistUtilityKind.None;
        }

        internal static void ResetKeypadUiState(KeypadGame game, DemolitionistKeypadNoOxyTask task) =>
            Keypad.ApplyDemolitionistKeypadUi(game, task);

        public static bool TryGetDemolitionistNumpadSession(KeypadGame? keypad, out DemolitionistKeypadNoOxyTask? demolitionistNumpadSabotage)
        {
            demolitionistNumpadSabotage = null;
            if (keypad == null || _task == null)
            {
                return false;
            }

            if (_minigame != null && SameMinigameInstance(keypad, _minigame))
            {
                demolitionistNumpadSabotage = _task;
                return true;
            }

            if (Minigame.Instance != null
                && SameMinigameInstance(keypad, Minigame.Instance)
                && keypad.TryCast<KeypadGame>() != null)
            {
                demolitionistNumpadSabotage = _task;
                return true;
            }

            return false;
        }

        public static bool OpenPlant(
            PlayerControl player,
            Vector2 position,
            int consoleKey,
            DemolitionistUtilityKind kind)
        {
            if (player == null || kind == DemolitionistUtilityKind.None)
            {
                return false;
            }

            CleanupTaskAndMinigame();
            _plantSucceeded = false;
            _plantPosition = position;
            _plantConsoleKey = consoleKey;
            _plantKind = kind;
            return Open(player, DemolitionistNumpadAction.Plant);
        }

        public static bool OpenDefuse(PlayerControl player)
        {
            CleanupTaskAndMinigame();
            return player != null && Open(player, DemolitionistNumpadAction.Defuse);
        }

        public static bool TryFinalizeSuccessfulKeypad(int numpadSessionId)
        {
            if (_numpadSessionCancelled
                || _action == DemolitionistNumpadAction.None
                || numpadSessionId != _openNumpadSessionId)
            {
                return false;
            }

            var local = PlayerControl.LocalPlayer;
            if (local == null)
            {
                CleanupTaskAndMinigame();
                return false;
            }

            var action = _action;

            if (action == DemolitionistNumpadAction.Plant)
            {
                // Defer the actual sabotage to the plant button's arming effect (countdown + shake).
                // The button already holds the captured plant position/kind from OnClick.
                _plantSucceeded = true;
            }
            else if (action == DemolitionistNumpadAction.Defuse)
            {
                DemolitionistSabotageState.RpcDefuseSabotage(local, local.PlayerId);
            }
            else
            {
                return false;
            }

            _minigame = null;
            CleanupTaskOnly();
            _numpadSessionCancelled = false;
            return true;
        }

        public static void Cancel(Minigame minigame)
        {
            if (_minigame == null || minigame == null || !SameMinigameInstance(_minigame, minigame))
            {
                return;
            }

            _numpadSessionCancelled = true;
            _minigame = null;
            CleanupTaskOnly();
        }

        public static void CancelActive()
        {
            var minigame = _minigame;
            if (minigame != null)
            {
                minigame.Close();
                return;
            }

            _numpadSessionCancelled = true;
            CleanupTaskAndMinigame();
        }

        private static bool SameMinigameInstance(Minigame? a, Minigame? b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.gameObject == b.gameObject)
            {
                return true;
            }

            return a.Pointer == b.Pointer;
        }

        private static bool Open(PlayerControl player, DemolitionistNumpadAction action)
        {
            _numpadSessionCancelled = false;

            DemolitionistUtilityConsoles.InvalidateKeypadPrefabCache();
            if (!DemolitionistUtilityConsoles.TryGetO2KeypadPrefab(out var prefab) || !prefab)
            {
                return false;
            }

            var parent = Camera.main
                ? Camera.main.transform
                : HudManager.Instance
                    ? HudManager.Instance.transform
                    : null;
            if (!parent)
            {
                return false;
            }

            var taskObject = new GameObject("DemolitionistKeypadTask");
            taskObject.transform.SetParent(player.transform);
            _task = taskObject.AddComponent<DemolitionistKeypadNoOxyTask>();
            _task.Owner = player;
            _task.targetNumber = UnityEngine.Random.Range(0, 100000);
            _openNumpadSessionId++;
            player.myTasks.Add(_task);

            _action = action;
            _minigame = Object.Instantiate(prefab, parent, false);
            if (_minigame == null)
            {
                CleanupTaskOnly();
                return false;
            }

            _minigame.transform.SetParent(parent, false);
            _minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            if (!_minigame.gameObject.activeSelf)
            {
                _minigame.gameObject.SetActive(true);
            }

            _minigame.Begin(_task);
            if (_minigame.TryCast<KeypadGame>() is KeypadGame keypad)
            {
                Keypad.ApplyDemolitionistKeypadUi(keypad, _task);
            }

            return true;
        }

        private static void CleanupTaskOnly()
        {
            _action = DemolitionistNumpadAction.None;
            _plantPosition = Vector2.zero;
            _plantConsoleKey = 0;
            _plantKind = DemolitionistUtilityKind.None;

            var task = _task;
            _task = null;

            if (task == null)
            {
                return;
            }

            if (task.Owner != null)
            {
                task.Owner.RemoveTask(task);
            }

            if (task.gameObject != null)
            {
                Object.Destroy(task.gameObject);
            }
        }

        private static void CleanupTaskAndMinigame()
        {
            var minigame = _minigame;
            _minigame = null;
            CleanupTaskOnly();
            if (minigame != null)
            {
                minigame.Close();
            }
        }
    }

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), [])]
    internal static class MinigameClose
    {
        private static void Postfix(Minigame __instance) => Controller.Cancel(__instance);
    }

    #endregion

    #region Keypad Harmony + fake O₂ task

    internal static class Keypad
    {
        internal static string FormatTargetCode(int targetNumber) =>
            targetNumber.ToString("D5", CultureInfo.InvariantCulture);

        internal static void ApplyDemolitionistKeypadUi(KeypadGame game, DemolitionistKeypadNoOxyTask task)
        {
            var tr = Traverse.Create(game);
            tr.Field<bool>("animating").Value = false;
            tr.Field<bool>("done").Value = false;
            game.numString = string.Empty;
            game.number = 0;

            var code = FormatTargetCode(task.targetNumber);
            var targetText = ResolveKeypadText(game, g => g.TargetText, "Target", "Note", "Sticky", "Code");
            if (targetText != null)
            {
                targetText.text = code;
            }

            var numberText = ResolveKeypadText(game, g => g.NumberText, "Number", "Input", "Entry");
            if (numberText != null)
            {
                numberText.text = string.Empty;
            }
        }

        private static TextMeshPro? ResolveKeypadText(
            KeypadGame game,
            Func<KeypadGame, TextMeshPro?> primary,
            params string[] nameHints)
        {
            var text = primary(game);
            if (text != null)
            {
                return text;
            }

            if (game == null || game.gameObject == null)
            {
                return null;
            }

            var all = game.GetComponentsInChildren<TextMeshPro>(true);
            foreach (var hint in nameHints)
            {
                foreach (var tmp in all)
                {
                    if (tmp != null && tmp.name.Contains(hint, StringComparison.OrdinalIgnoreCase))
                    {
                        return tmp;
                    }
                }
            }

            return null;
        }

        private static void SetNumberText(KeypadGame game, string value)
        {
            var numberText = ResolveKeypadText(game, g => g.NumberText, "Number", "Input", "Entry");
            if (numberText != null)
            {
                numberText.text = value;
            }
        }

        public static void Register(Harmony harmony, ManualLogSource log)
        {
            var enterPrefix = AccessTools.Method(typeof(Keypad), nameof(KeypadGameEnterPrefix));
            var enter = AccessTools.Method(typeof(KeypadGame), nameof(KeypadGame.Enter));
            if (enterPrefix != null && enter != null)
            {
                harmony.Patch(enter, prefix: new HarmonyMethod(enterPrefix));
            }
            else
            {
                log.LogError("Demolitionist keypad: failed to patch KeypadGame.Enter.");
            }

            var beginPostfix = AccessTools.Method(typeof(Keypad), nameof(KeypadGameBeginPostfix));
            var begin = AccessTools.Method(typeof(KeypadGame), nameof(KeypadGame.Begin));
            if (beginPostfix != null && begin != null)
            {
                harmony.Patch(begin, postfix: new HarmonyMethod(beginPostfix));
            }

            var clickNumberPrefix = AccessTools.Method(typeof(Keypad), nameof(KeypadClickNumberPrefix));
            var clickNumber = AccessTools.Method(typeof(KeypadGame), nameof(KeypadGame.ClickNumber), new[] { typeof(int) });
            if (clickNumberPrefix != null && clickNumber != null)
            {
                harmony.Patch(clickNumber, prefix: new HarmonyMethod(clickNumberPrefix));
            }
            else
            {
                log.LogError("Demolitionist keypad: failed to patch KeypadGame.ClickNumber.");
            }

            var clearEntryPrefix = AccessTools.Method(typeof(Keypad), nameof(KeypadClearEntryPrefix));
            var clearEntry = AccessTools.Method(typeof(KeypadGame), nameof(KeypadGame.ClearEntry));
            if (clearEntryPrefix != null && clearEntry != null)
            {
                harmony.Patch(clearEntry, prefix: new HarmonyMethod(clearEntryPrefix));
            }
        }

        [HarmonyPatch(typeof(NoOxyTask), nameof(NoOxyTask.Initialize))]
        [HarmonyPrefix]
        private static bool NoOxyTaskInitializePrefix(NoOxyTask __instance)
        {
            if (__instance is not DemolitionistKeypadNoOxyTask tk)
            {
                return true;
            }

            // Open() assigns targetNumber before Begin; do not re-roll or validation won't match TargetText.

            if (ShipStatus.Instance != null
                && ShipStatus.Instance.Systems != null
                && ShipStatus.Instance.Systems.TryGetValue(SystemTypes.LifeSupp, out var sys))
            {
                var lifeSupp = sys?.TryCast<LifeSuppSystemType>();
                if (lifeSupp != null)
                {
                    Traverse.Create(tk).Field("reactor").SetValue(lifeSupp);
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(NoOxyTask), "FixedUpdate")]
        [HarmonyPrefix]
        private static bool NoOxyTaskFixedUpdatePrefix(NoOxyTask __instance) =>
            __instance is not DemolitionistKeypadNoOxyTask;

        private static void KeypadGameBeginPostfix(KeypadGame __instance)
        {
            if (!Controller.TryGetDemolitionistNumpadSession(__instance, out var task) || task == null)
            {
                return;
            }

            ApplyDemolitionistKeypadUi(__instance, task);
        }

        private static bool KeypadClickNumberPrefix(KeypadGame __instance, int i)
        {
            if (!Controller.TryGetDemolitionistNumpadSession(__instance, out _))
            {
                return true;
            }

            var tr = Traverse.Create(__instance);
            tr.Field<bool>("animating").Value = false;
            tr.Field<bool>("done").Value = false;

            var current = __instance.numString?.ToString() ?? string.Empty;
            if (current.Length >= 5)
            {
                return false;
            }

            var next = current + i.ToString(CultureInfo.InvariantCulture);
            __instance.numString = next;
            if (int.TryParse(next, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                __instance.number = parsed;
            }

            SetNumberText(__instance, next);

            return false;
        }

        private static bool KeypadClearEntryPrefix(KeypadGame __instance)
        {
            if (!Controller.TryGetDemolitionistNumpadSession(__instance, out _))
            {
                return true;
            }

            var tr = Traverse.Create(__instance);
            tr.Field<bool>("animating").Value = false;
            tr.Field<bool>("done").Value = false;
            __instance.numString = string.Empty;
            __instance.number = 0;
            SetNumberText(__instance, string.Empty);

            return false;
        }

        private static bool KeypadGameEnterPrefix(KeypadGame __instance)
        {
            if (!Controller.TryGetDemolitionistNumpadSession(__instance, out var demolitionistNumpadSabotage)
                || demolitionistNumpadSabotage == null)
            {
                return true;
            }

            var entered = ReadEnteredCode(__instance);
            var correct = demolitionistNumpadSabotage.targetNumber == entered;
            var sessionId = Controller.ActiveOpenNumpadSessionId;
            __instance.StartCoroutine(
                AnimateDemolitionistKeypad(__instance, correct, sessionId).WrapToIl2Cpp());
            return false;
        }

        private static int ReadEnteredCode(KeypadGame game)
        {
            var ns = game.numString?.ToString();
            if (!string.IsNullOrEmpty(ns)
                && int.TryParse(ns, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            try
            {
                return Traverse.Create(game).Field<int>("number").Value;
            }
            catch
            {
                return game.number;
            }
        }

        private static IEnumerator AnimateDemolitionistKeypad(KeypadGame game, bool correct, int numpadSessionId)
        {
            var tr = Traverse.Create(game);
            tr.Field<bool>("animating").Value = true;
            var wait = new WaitForSeconds(0.1f);
            yield return wait;
            SetNumberText(game, string.Empty);
            yield return wait;

            if (correct)
            {
                var applied = Controller.TryFinalizeSuccessfulKeypad(numpadSessionId);
                if (applied)
                {
                    var successText =
                        MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Numpad.Success");

                    tr.Field<bool>("done").Value = true;

                    SetNumberText(game, successText);
                    yield return wait;

                    SetNumberText(game, string.Empty);
                    yield return wait;

                    SetNumberText(game, successText);
                    yield return wait;

                    SetNumberText(game, string.Empty);
                    yield return wait;

                    SetNumberText(game, successText);
                }
            }
            else
            {
                var incorrectText =
                    MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Numpad.Incorrect");

                SetNumberText(game, incorrectText);
                yield return wait;

                SetNumberText(game, string.Empty);
                yield return wait;

                SetNumberText(game, incorrectText);
                yield return wait;

                game.numString = string.Empty;
                game.number = 0;
                SetNumberText(game, string.Empty);
            }

            tr.Field<bool>("animating").Value = false;
            if (correct)
            {
                game.Close();
            }
        }
    }

    [RegisterInIl2Cpp]
    public sealed class DemolitionistKeypadNoOxyTask(nint cppPtr) : NoOxyTask(cppPtr)
    {
        public override int TaskStep => 0;

        public override bool IsComplete => false;

        public override void AppendTaskText(StringBuilder sb)
        {
            // Not used here.
        }

        public override bool ValidConsole(global::Console console) => true;

        public override void Complete()
        {
            if (Owner != null)
            {
                Owner.RemoveTask(this);
            }

            if (gameObject != null)
            {
                Object.Destroy(gameObject);
            }
        }
    }

    #endregion
}
