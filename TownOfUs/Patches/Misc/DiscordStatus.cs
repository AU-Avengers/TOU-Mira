using BepInEx.Unity.IL2CPP;
using Discord;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TownOfUs.Patches.Misc;
// Patch taken from https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs
[HarmonyPatch]
public static class DiscordStatus
{
    private const long ClientId = 1380592659000721489;
    private const uint SteamAppId = 945360;
    private static string ModInfo = $"TOU:M v{TownOfUsPlugin.Version}" + (TownOfUsPlugin.IsDevBuild ? " (DEV)" : string.Empty);
    private static string ModCount = "0 Mods";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.Start))]
    public static bool DiscordManagerStartPrefix(DiscordManager __instance)
    {
        DiscordManager.ClientId = ClientId;
        if (Application.platform == RuntimePlatform.Android)
        {
            return true;
        }

        InitializeDiscord(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public static void ActivityManagerUpdateActivityPrefix(ActivityManager __instance, [HarmonyArgument(0)] Activity activity)
    {
        activity.Details = (string.IsNullOrEmpty(activity.Details)) ? ModInfo : ModInfo + " | " + activity.Details;
        activity.State = (string.IsNullOrEmpty(activity.State)) ? ModCount : $"{ModCount} | {activity.State}";
        activity.Assets.LargeImage = "icon";
    }

    private static void InitializeDiscord(DiscordManager __instance)
    {
        __instance.presence = new Discord.Discord(ClientId, 1UL);
        var activityManager = __instance.presence.GetActivityManager();

        activityManager.RegisterSteam(SteamAppId);
        activityManager.add_OnActivityJoin((Action<string>)__instance.HandleJoinRequest);
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
        {
            ModCount = $"{IL2CPPChainloader.Instance.Plugins.Count} Mods";
            __instance.OnSceneChange(scene.name);
        }));
        __instance.SetInMenus();
    }
}