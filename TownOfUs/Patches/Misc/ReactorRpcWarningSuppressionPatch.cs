using HarmonyLib;
using BepInEx.Logging;

namespace TownOfUs.Patches.Misc;

/// <summary>
///     Suppresses Reactor warnings about non-immediate RPCs being removed.
///     These warnings spam the console but are harmless since Reactor now always sends immediately.
/// </summary>
[HarmonyPatch]
public static class ReactorRpcWarningSuppressionPatch
{
    [HarmonyPatch(typeof(ManualLogSource), nameof(ManualLogSource.LogWarning), typeof(object))]
    [HarmonyPrefix]
    public static bool LogWarningPrefix(ManualLogSource __instance, object data)
    {
        if (__instance.SourceName == "Reactor" &&
            data.ToString()!.Contains("Non-immediate RPCs were removed"))
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ManualLogSource), nameof(ManualLogSource.Log), typeof(LogLevel), typeof(object))]
    [HarmonyPrefix]
    public static bool LogPrefix(ManualLogSource __instance, LogLevel level, object data)
    {
        if (level == LogLevel.Warning &&
            __instance.SourceName == "Reactor" &&
            data.ToString()!.Contains("Non-immediate RPCs were removed"))
        {
            return false;
        }

        return true;
    }
}

