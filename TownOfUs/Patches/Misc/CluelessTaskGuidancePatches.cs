using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

/// <summary>
/// Clueless modifier: hides task list, task arrows/markers, and map task overlay for the local player.
/// </summary>
[HarmonyPatch]
public static class CluelessTaskGuidancePatches
{
    private static bool LocalIsClueless()
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<CluelessModifier>();
    }

    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
    [HarmonyPrefix]
    public static bool TaskPanelSetTaskTextPrefix(TaskPanelBehaviour __instance)
    {
        if (!LocalIsClueless())
        {
            return true;
        }

        // Only suppress the vanilla task panel (don't break TOU's separate "RolePanel").
        if (HudManager.Instance != null && HudManager.Instance.TaskPanel == __instance)
        {
            if (__instance.taskText != null)
            {
                __instance.taskText.text = string.Empty;
            }
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrowAndLocation))]
    [HarmonyPrefix]
    public static bool NormalPlayerTaskUpdateArrowAndLocationPrefix(NormalPlayerTask __instance)
    {
        if (!LocalIsClueless() || __instance == null)
        {
            return true;
        }

        // Only disable guidance for the local player's own tasks.
        if (__instance.Owner != PlayerControl.LocalPlayer)
        {
            return true;
        }

        // Best-effort cleanup: if an arrow already exists, destroy it so it can't linger.
        TryDestroyExistingTaskArrow(__instance);
        return false;
    }

    private static void TryDestroyExistingTaskArrow(NormalPlayerTask task)
    {
        try
        {
            var t = task.GetType();
            var field =
                AccessTools.Field(t, "Arrow") ??
                AccessTools.Field(t, "arrow") ??
                AccessTools.Field(t, "taskArrow") ??
                AccessTools.Field(t, "_arrow");

            if (field == null)
            {
                return;
            }

            var arrowObj = field.GetValue(task) as MonoBehaviour;
            if (arrowObj == null)
            {
                return;
            }

            if (arrowObj.gameObject != null && arrowObj.gameObject.activeSelf)
            {
                arrowObj.gameObject.Destroy();
            }

            // Null out the field so other callers can't reuse it.
            field.SetValue(task, null);
        }
        catch
        {
            // ignored: IL2CPP field names may differ by AU version
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowCountOverlay))]
    [HarmonyPostfix]
    public static void MapBehaviourShowPostfix(MapBehaviour __instance)
    {
        if (!LocalIsClueless() || __instance == null)
        {
            return;
        }

        // Hide task locations on the map for Clueless.
        __instance.taskOverlay?.Hide();
    }
}


