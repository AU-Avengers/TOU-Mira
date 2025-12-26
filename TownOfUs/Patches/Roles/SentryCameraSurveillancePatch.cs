using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class SentryCameraSurveillancePatch
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("SentryCameraSurveillancePatch");

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void SurveillanceMinigameBeginPostfix(SurveillanceMinigame __instance)
    {
        Logger.LogInfo("[BEGIN] SurveillanceMinigame.Begin called");

        Logger.LogInfo("=== CAMERA BLINKING DEBUG: SurveillanceMinigame ===");
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

        SentryCameraUiUtilities.ResetPageState();
        SentryCameraUiUtilities.TryCachePolusPagingUi();
        SentryCameraMinigameUtilities.SwapAllCamerasForNonSentry(__instance);
        SentryCameraMinigameUtilities.AddSentryCameras(__instance);
        SentryCameraUiUtilities.EnsureSkeldPagingButtons(__instance);
        SentryCameraPortablePatch.ApplyPortableBlinkState();
        Logger.LogInfo("[BEGIN] SurveillanceMinigame.Begin completed");
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    [HarmonyPrefix]
    public static bool SurveillanceMinigameUpdatePrefix(SurveillanceMinigame __instance)
    {
        var ship = ShipStatus.Instance;
        if (ship == null || ship.AllCameras == null || ship.AllCameras.Length <= 4)
        {
            return true;
        }

        SentryCameraMinigameUtilities.AddSentryCameras(__instance);

        SentryCameraUiUtilities.UiRepairTimer += Time.deltaTime;
        var numberOfPages = Mathf.CeilToInt(ship.AllCameras.Length / 4f);
        if (numberOfPages <= 0)
        {
            numberOfPages = 1;
        }

        var currentPage = SentryCameraUiUtilities.CurrentPage;
        currentPage %= numberOfPages;
        if (currentPage < 0) currentPage += numberOfPages;
        SentryCameraUiUtilities.CurrentPage = currentPage;

        var update = false;

        try
        {
            if (numberOfPages > 1 && Input.GetMouseButtonDown(0) && Camera.main != null)
            {
                var viewables = __instance.Viewables != null ? __instance.Viewables.transform : null;
                if (viewables != null)
                {
                    var rightTf = viewables.Find("SentryRightArrow");
                    var leftTf = viewables.Find("SentryLeftArrow");
                    var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorld.z = 0f;

                    var clickedRight = SentryCameraUiUtilities.IsMouseOverSprite(rightTf, mouseWorld);
                    var clickedLeft = !clickedRight && SentryCameraUiUtilities.IsMouseOverSprite(leftTf, mouseWorld);

                    if (clickedRight || clickedLeft)
                    {
                        SentryCameraUiUtilities.SuppressCloseFrame = Time.frameCount;
                        if (clickedRight)
                        {
                            SentryCameraUiUtilities.CurrentPage = (currentPage + 1) % numberOfPages;
                            Logger.LogInfo($"[MANUAL CLICK] Right page arrow. New page={SentryCameraUiUtilities.CurrentPage}, frame={Time.frameCount}");
                        }
                        else
                        {
                            SentryCameraUiUtilities.CurrentPage = (currentPage + numberOfPages - 1) % numberOfPages;
                            Logger.LogInfo($"[MANUAL CLICK] Left page arrow. New page={SentryCameraUiUtilities.CurrentPage}, frame={Time.frameCount}");
                        }

                        try
                        {
                            var clip = SentryCameraUiUtilities.GetPageFlipSound() ?? HudManager.Instance?.MapButton?.ClickSound;
                            if (clip != null && Constants.ShouldPlaySfx())
                            {
                                SoundManager.Instance.PlaySound(clip, false, 1f, null);
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        update = true;
                        SentryCameraUiUtilities.ForceRefresh = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[MANUAL CLICK] Exception: {ex}");
        }

        if (SentryCameraUiUtilities.ForceRefresh)
        {
            update = true;
            SentryCameraUiUtilities.ForceRefresh = false;
            Logger.LogInfo($"[PAGE FLIP] Page changed to {SentryCameraUiUtilities.CurrentPage}, frame {Time.frameCount}");
        }

        if (SentryCameraUiUtilities.UiRepairTimer > 1.0f)
        {
            SentryCameraUiUtilities.UiRepairTimer = 0f;
            try
            {
                var viewables = __instance.Viewables != null ? __instance.Viewables.transform : null;
                if (viewables != null)
                {
                    var right = viewables.Find("SentryRightArrow");
                    var left = viewables.Find("SentryLeftArrow");
                    var needsRepair = right == null || left == null;

                    if (needsRepair)
                    {
                        Logger.LogWarning("[PERIODIC REPAIR] Buttons missing! Right: " + (right == null ? "NULL" : "EXISTS") + ", Left: " + (left == null ? "NULL" : "EXISTS"));
                    }

                    if (!needsRepair && right != null)
                    {
                        var sr = right.GetComponent<SpriteRenderer>();
                        if (sr == null || !sr.enabled || !right.gameObject.activeSelf)
                        {
                            Logger.LogWarning($"[PERIODIC REPAIR] Right button broken! SR={(sr != null ? $"enabled={sr.enabled}" : "NULL")}, Active={right.gameObject.activeSelf}");
                            needsRepair = true;
                        }
                    }
                    if (!needsRepair && left != null)
                    {
                        var sr = left.GetComponent<SpriteRenderer>();
                        if (sr == null || !sr.enabled || !left.gameObject.activeSelf)
                        {
                            Logger.LogWarning($"[PERIODIC REPAIR] Left button broken! SR={(sr != null ? $"enabled={sr.enabled}" : "NULL")}, Active={left.gameObject.activeSelf}");
                            needsRepair = true;
                        }
                    }

                    if (needsRepair)
                    {
                        Logger.LogInfo("[PERIODIC REPAIR] Triggering repair...");
                        SentryCameraUiUtilities.EnsureSkeldPagingButtons(__instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[PERIODIC REPAIR] Exception: {ex}");
            }
        }

        SentryCameraUiUtilities.UpdateSkeldDotIndicator(__instance, numberOfPages);

        var sabotaged = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer);

        if ((__instance.isStatic || update) && !sabotaged)
        {
            __instance.isStatic = false;
            var textures = __instance.textures;
            if (textures == null)
            {
                return false;
            }

            var page = SentryCameraUiUtilities.CurrentPage;
            for (var i = 0; i < __instance.ViewPorts.Length; i++)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                __instance.SabText[i].gameObject.SetActive(false);

                var idx = page * 4 + i;
                if (idx < textures.Length && textures[idx] != null)
                {
                    __instance.ViewPorts[i].material.SetTexture("_MainTex", textures[idx]);
                }
                else
                {
                    __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                }
            }
        }
        else if (!__instance.isStatic && sabotaged)
        {
            __instance.isStatic = true;
            for (var i = 0; i < __instance.ViewPorts.Length; i++)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[i].gameObject.SetActive(true);
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(Minigame))]
    public static class MinigameCloseForceCloseSuppressPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(Minigame))
                .Where(m => m.Name is nameof(Minigame.Close) or nameof(Minigame.ForceClose));
        }

        public static bool Prefix(Minigame __instance)
        {
            if (__instance is SurveillanceMinigame && Time.frameCount == SentryCameraUiUtilities.SuppressCloseFrame)
            {
                Logger.LogInfo($"[CLOSE SUPPRESS] Suppressing close at frame {Time.frameCount} (suppress frame was {SentryCameraUiUtilities.SuppressCloseFrame})");
                return false;
            }

            return true;
        }

        public static void Postfix(Minigame __instance)
        {
            SentryCameraMinigameUtilities.RestoreAllCameras(__instance);

            try
            {
                SentryPortableCameraButtonBase.HandleMinigameClosedStatic(__instance);
            }
            catch
            {
               // ignored
            }

            SentryCameraPortablePatch.ApplyPortableBlinkState();
        }
    }

    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
    public static class PassiveButtonClickPatch
    {
        public static void Postfix(PassiveButton __instance)
        {
            if (__instance == null) return;
            var name = __instance.gameObject?.name ?? "NULL";
            if (name.Contains("SentryRightArrow") || name.Contains("SentryLeftArrow"))
            {
                Logger.LogInfo($"[PASSIVE BUTTON] ReceiveClickDown called on {name}, frame {Time.frameCount}, enabled={__instance.enabled}, OnClick={(__instance.OnClick != null ? "EXISTS" : "NULL")}");
            }
        }
    }
}