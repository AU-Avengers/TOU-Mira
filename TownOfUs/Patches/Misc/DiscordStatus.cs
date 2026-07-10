using BepInEx.Unity.IL2CPP;
using Discord;
using HarmonyLib;

namespace TownOfUs.Patches.Misc;
/// <remarks>
/// Patch taken from <see href="https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs"/>
/// </remarks>
[HarmonyPatch(typeof(ActivityManager))]
public static class DiscordStatus
{
    private static readonly string ModInfo = $"TOU:M v{TownOfUsPlugin.Version}" + (TownOfUsPlugin.IsDevBuild && !TownOfUsPlugin.Version.Contains("beta") ? " (DEV)" : string.Empty);
    private static readonly string _smallIcon = "???";

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ActivityManager.UpdateActivity))]
    public static void ActivityManagerUpdateActivityPrefix([HarmonyArgument(0)] Activity activity)
    {
        var modCount = $"{IL2CPPChainloader.Instance.Plugins.Count} Mods";
        activity.Details = (string.IsNullOrEmpty(activity.Details)) ? ModInfo : ModInfo + " | " + activity.Details;
        activity.State = (string.IsNullOrEmpty(activity.State)) ? modCount : $"{modCount} | {activity.State}";
        activity.Assets.LargeImage = "icon";
        activity.Assets.SmallImage = _smallIcon;
    }
}