using BepInEx.Logging;
using HarmonyLib;
using TownOfUs.Utilities;
using UnityEngine;
using TMPro;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class SentryCameraMinigamePatch
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("SentryCameraMinigamePatch");

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void FungleSurveillanceMinigameBeginPostfix(FungleSurveillanceMinigame __instance)
    {
        SentryCameraMinigameUtilities.SwapAllCamerasForNonSentry(__instance);
        SentryCameraMinigameUtilities.AddSentryCameras(__instance);
    }

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void PlanetSurveillanceMinigameBeginPostfix(PlanetSurveillanceMinigame __instance)
    {
        Logger.LogInfo("PlanetSurveillanceMinigameBeginPostfix CALLED!");

        if (__instance == null)
        {
            Logger.LogError("__instance is null!");
            return;
        }

        Logger.LogInfo("=== CAMERA BLINKING DEBUG: PlanetSurveillanceMinigame ===");
        Logger.LogInfo($"Minigame MyTask: {__instance.MyTask?.GetType().FullName ?? "NULL"}");
        Logger.LogInfo($"Minigame MyNormTask: {__instance.MyNormTask?.GetType().FullName ?? "NULL"}");
        if (__instance.MyNormTask != null)
        {
            Logger.LogInfo($"MyNormTask TaskType: {__instance.MyNormTask.TaskType}");
            Logger.LogInfo($"MyNormTask IsComplete: {__instance.MyNormTask.IsComplete}");
        }
        Logger.LogInfo($"Minigame Instance: {Minigame.Instance?.GetType().FullName ?? "NULL"}");
        Logger.LogInfo($"Minigame Instance == __instance: {Minigame.Instance == __instance}");

        if (PlayerControl.LocalPlayer != null)
        {
            Logger.LogInfo($"Player task count: {PlayerControl.LocalPlayer.myTasks.Count}");
            for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
            {
                var task = PlayerControl.LocalPlayer.myTasks[i];
                if (task != null)
                {
                    var normTask = task.TryCast<NormalPlayerTask>();
                    if (normTask != null)
                    {
                        Logger.LogInfo($"  Task {i}: TaskType={normTask.TaskType}, IsComplete={normTask.IsComplete}");
                    }
                    else
                    {
                        Logger.LogInfo($"  Task {i}: {task.GetType().FullName}");
                    }
                }
            }
        }
        Logger.LogInfo("=== END CAMERA BLINKING DEBUG ===");

        try
        {
            SentryCameraUiUtilities.CachePolusPagingUi(__instance);
            LogPolusSecurityUi(__instance);
            SentryCameraMinigameUtilities.SwapAllCamerasForNonSentry(__instance);
            SentryCameraMinigameUtilities.AddSentryCameras(__instance);
            SentryCameraPortablePatch.ApplyPortableBlinkState();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception in PlanetSurveillanceMinigameBeginPostfix: {ex}");
            Logger.LogError($"Stack trace: {ex.StackTrace}");
        }
    }

    private static void LogPolusSecurityUi(PlanetSurveillanceMinigame minigame)
    {
        try
        {
            Logger.LogInfo("LogPolusSecurityUi called!");

            if (minigame == null)
            {
                Logger.LogError("minigame is null!");
                return;
            }

            Logger.LogInfo("=== POLUS SECURITY UI DEBUG LOG ===");
            Logger.LogInfo($"Minigame type: {minigame.GetType().FullName}");
            Logger.LogInfo($"Minigame name: {minigame.name}");

            var buttons = minigame.GetComponentsInChildren<PassiveButton>(true);
            Logger.LogInfo($"\nFound {buttons.Length} PassiveButton(s):");
            for (var i = 0; i < buttons.Length; i++)
            {
                var btn = buttons[i];
                if (btn == null) continue;

                var name = btn.name;
                var sr = btn.GetComponent<SpriteRenderer>();
                var spriteName = sr != null && sr.sprite != null ? sr.sprite.name : "NULL";
                var soundName = btn.ClickSound != null ? btn.ClickSound.name : "NULL";
                var pos = btn.transform.position;

                Logger.LogInfo($"  [{i}] {name}");
                Logger.LogInfo($"      Sprite: {spriteName}");
                Logger.LogInfo($"      ClickSound: {soundName}");
                Logger.LogInfo($"      Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");

                var texts = btn.GetComponentsInChildren<TextMeshPro>(true);
                if (texts.Length > 0)
                {
                    Logger.LogInfo($"      TextMeshPro children: {texts.Length}");
                    foreach (var txt in texts)
                    {
                        if (txt != null)
                        {
                            Logger.LogInfo($"        - {txt.name}: \"{txt.text}\" (fontSize: {txt.fontSize})");
                        }
                    }
                }
            }

            var sprites = minigame.GetComponentsInChildren<SpriteRenderer>(true);
            Logger.LogInfo($"\nFound {sprites.Length} SpriteRenderer(s):");
            foreach (var sr in sprites)
            {
                if (sr == null || sr.sprite == null) continue;
                var name = sr.name;
                var spriteName = sr.sprite.name;
                var pos = sr.transform.position;
                Logger.LogInfo($"  - {name}: sprite=\"{spriteName}\" pos=({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
            }

            var allTexts = minigame.GetComponentsInChildren<TextMeshPro>(true);
            Logger.LogInfo($"\nFound {allTexts.Length} TextMeshPro(s):");
            foreach (var txt in allTexts)
            {
                if (txt == null) continue;
                var name = txt.name;
                var text = txt.text;
                var fontSize = txt.fontSize;
                var pos = txt.transform.position;
                Logger.LogInfo($"  - {name}: \"{text}\" fontSize={fontSize} pos=({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
            }

            var clips = minigame.GetComponentsInChildren<AudioSource>(true);
            Logger.LogInfo($"\nFound {clips.Length} AudioSource(s):");
            foreach (var src in clips)
            {
                if (src == null || src.clip == null) continue;
                Logger.LogInfo($"  - {src.name}: clip=\"{src.clip.name}\"");
            }

            Logger.LogInfo("=== END POLUS SECURITY UI DEBUG LOG ===");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception in LogPolusSecurityUi: {ex}");
            Logger.LogError($"Stack trace: {ex.StackTrace}");
        }
    }
}