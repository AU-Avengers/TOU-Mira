using HarmonyLib;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

// Simplified version of the code from https://github.com/D1GQ/BetterAmongUs/blob/main/src/Patches/Client/SplashIntroPatch.cs
// This just removes the jarry intro and replaces the Play Every Ware logo rather than replace the Innersloth logo.
[HarmonyPatch]
internal static class SplashIntroPatch
{
    public static bool IntroSetup;
    public static GameObject SlothLogo;
    public static GameObject PewLogo;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    [HarmonyPrefix]
    private static void SplashManager_Start_Prefix(SplashManager __instance)
    {
        // Reset all flags when splash screen starts
        IntroSetup = false;

        // Hide black overlay by moving it out of view
        __instance.logoAnimFinish.transform.Find("BlackOverlay").transform.SetLocalY(100f);
    }

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    [HarmonyPrefix]
    private static bool UpdatePrefix(SplashManager __instance)
    {
        // After 1.8 seconds in BAU intro, remove audio to prevent overlap
        if (Time.time - __instance.startTime > 5.3f && IntroSetup)
        {
            UnityEngine.Object.Destroy(__instance.logoAnimFinish.GetComponent<AudioSource>());
        }

        // When game data is loaded and minimum time has passed
        if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange)
        {
            if (!IntroSetup)
            {
                SetUpSplash(__instance);
                return false;
            }

            // Check if BAU intro has completed
            if (Time.time - __instance.startTime > 5.5f && IntroSetup)
            {
                // Allow scene transition to proceed
                __instance.sceneChanger.AllowFinishLoadingScene();
                __instance.startedSceneLoad = true;
                //__instance.loadingObject.SetActive(true);
            }
        }

        // Return false to prevent original Update from running (we handle everything)
        return false;
    }

    public static void SetUpSplash(SplashManager instance)
    {
        instance.startTime = Time.time;
        instance.logoAnimFinish.gameObject.SetActive(false);
        instance.logoAnimFinish.gameObject.SetActive(true);

        // Replace InnerSloth logo with BAU logo
        SlothLogo = instance.logoAnimFinish.transform.Find("LogoRoot/ISLogo").gameObject;
        PewLogo = instance.logoAnimFinish.transform.Find("LogoRoot/PEWLogo").gameObject;
        PewLogo.GetComponent<ConditionalHide>().Destroy();
        PewLogo.active = true;
        PewLogo.GetComponent<SpriteRenderer>().sprite = TouAssets.AuAvengersLogo.LoadAsset();
        SlothLogo.transform.localPosition -= new Vector3(4f, 0f, 0f);
        PewLogo.transform.localPosition -= new Vector3(0.5f, 0f, 0f);

        // Show black overlay
        instance.logoAnimFinish.transform.Find("BlackOverlay").transform.SetLocalY(0f);

        IntroSetup = true;
    }
}