using BepInEx.Bootstrap;
using Discord;
using HarmonyLib;

namespace TownOfUs.Patches.Misc;
/// <remarks>
/// Patch taken from <see href="https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs"/>
/// </remarks>
[HarmonyPatch(typeof(ActivityManager))]
public static class DiscordStatus
{
    private static string ModInfo = $"TOU:M v{TownOfUsPlugin.Version}";
    private static string _smallIcon = "???";

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ActivityManager.UpdateActivity))]
    public static void ActivityManagerUpdateActivityPrefix(ActivityManager __instance, [HarmonyArgument(0)] Activity activity)
    {
        var modCount = $"{Chainloader.PluginInfos.Count} Mods";
        activity.Details = (string.IsNullOrEmpty(activity.Details)) ? ModInfo : ModInfo + " | " + activity.Details;
        activity.State = (string.IsNullOrEmpty(activity.State)) ? modCount : $"{modCount} | {activity.State}";
        activity.Assets.LargeImage = "icon";
        activity.Assets.SmallImage = _smallIcon;
    }
}