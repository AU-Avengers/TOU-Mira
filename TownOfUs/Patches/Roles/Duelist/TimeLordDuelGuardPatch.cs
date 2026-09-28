using System;
using System.Reflection;
using HarmonyLib;
using TownOfUs.Modules.Duelist;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class TimeLordDuelGuardPatch
{
    [HarmonyPatch(typeof(TimeLordRewindSystem), nameof(TimeLordRewindSystem.RecordLocalSnapshot))]
    [HarmonyPrefix]
    public static bool SkipSnapshotWhileDueling()
    {
        var lp = PlayerControl.LocalPlayer;
        return lp == null || !DuelManager.IsInDuel(lp.PlayerId);
    }

    [HarmonyPatch(typeof(TimeLordRewindSystem), nameof(TimeLordRewindSystem.TryHandleRewindPhysics))]
    [HarmonyPrefix]
    public static bool ExemptDuellistFromRewind(ref bool __result)
    {
        var lp = PlayerControl.LocalPlayer;
        if (lp == null || lp.Data == null || lp.Data.IsDead || !DuelManager.IsInDuel(lp.PlayerId))
        {
            return true;
        }

        lp.moveable = true;
        if (lp.Collider != null && !lp.Collider.enabled)
        {
            lp.Collider.enabled = true;
        }

        if (Time.time >= TimeLordRewindSystem.RewindEndTime)
        {
            TimeLordRewindSystem.StopRewind();
        }

        __result = false;
        return false;
    }
    [HarmonyPatch(typeof(TimeLordRewindSystem), nameof(TimeLordRewindSystem.ReviveFromRewind))]
    [HarmonyPrefix]
    public static bool SkipReviveForDuelDeaths(PlayerControl revived)
    {
        return revived == null || !DuelManager.DiedInDuel(revived.PlayerId);
    }

    private static float _hostScheduleUntil;

    [HarmonyPatch(typeof(TimeLordRewindSystem), nameof(TimeLordRewindSystem.StartRewind))]
    [HarmonyPostfix]
    public static void CaptureHostRewindWindow(float duration)
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            _hostScheduleUntil = Time.time + duration + 0.25f;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPostfix]
    public static void DriveHostSchedulerDuringRewind(PlayerPhysics __instance)
    {
        if (_hostScheduleUntil <= 0f)
        {
            return;
        }

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            __instance == null || __instance.myPlayer != PlayerControl.LocalPlayer)
        {
            return;
        }

        if (Time.time >= _hostScheduleUntil)
        {
            _hostScheduleUntil = 0f;
            return;
        }
        InvokeHostScheduler();
    }

    private static readonly MethodInfo? HostScheduler =
        AccessTools.Method(typeof(TimeLordRewindSystem), "ProcessHostScheduledRewindActions");

    private static void InvokeHostScheduler()
    {
        try
        {
            HostScheduler?.Invoke(null, null);
        }
        catch (Exception)
        {
        }
    }

    private static readonly FieldInfo? BufferField =
        AccessTools.Field(typeof(TimeLordRewindSystem), "Buffer");

    private static readonly FieldInfo? TaskBufferField =
        AccessTools.Field(typeof(TimeLordRewindSystem), "TaskBuffer");

    public static void ClearLocalRewindHistory()
    {
        try
        {
            ClearBuffer(BufferField);
            ClearBuffer(TaskBufferField);
        }
        catch (Exception)
        {

        }
    }

    private static void ClearBuffer(FieldInfo? field)
    {
        var buffer = field?.GetValue(null);
        if (buffer != null)
        {
            AccessTools.Method(buffer.GetType(), "Clear")?.Invoke(buffer, null);
        }
    }
}
