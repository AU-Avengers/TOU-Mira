using HarmonyLib;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
internal static class PingTrackerPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    public static void Postfix(PingTracker __instance)
    {
        var extraText = $"<align=center><size=60%><space=3em>Region: <color=#C96DFF>{MiscUtils.GetRegionName()}</color></size></align>";
        __instance.text.text += $"\r\n{extraText}";
    }
}