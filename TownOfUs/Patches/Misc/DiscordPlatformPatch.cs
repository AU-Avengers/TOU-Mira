using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TownOfUs.Patches.Misc;
/// <remarks>
/// Patch taken from <see href="https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs"/>
/// </remarks>
[HarmonyPatch]
public static class DiscordPlatformPatch
{
    private const long ClientId = 1380592659000721489;
    private const uint SteamAppId = 945360;

    public static bool Prepare()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return false;
        }
        
        return true;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return new List<MethodBase>(); 
        }
        
        return new List<MethodBase>
        {
            AccessTools.Method(typeof(DiscordManager), "Start")
        };
    }

    public static bool Prefix(DiscordManager __instance)
    {
        try
        {
            __instance.presence = new Discord.Discord(ClientId, 1UL);
            var activityManager = __instance.presence.GetActivityManager();

            activityManager.RegisterSteam(SteamAppId);
            activityManager.OnActivityJoin += __instance.HandleJoinRequest;
            SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>) ((scene, mode) => __instance.OnSceneChange(scene.name));
            __instance.SetInMenus();
        }
        catch
        {
            // ignore
        }
        return false;
    }
}