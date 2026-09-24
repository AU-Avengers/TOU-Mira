using HarmonyLib;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
internal static class PingTrackerPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PingTracker __instance)
    {
        var regionText = MiraLocaleManager.Get(
            "TouRegion",
            "Region");

        var extraText =
            $"<align=center><size=60%><space=3em>{regionText}: <color=#C96DFF>{MiscUtils.GetRegionName()}</color></size></align>";

        __instance.text.text += $"\r\n{extraText}";
    }
}