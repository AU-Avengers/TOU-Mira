using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(LoadingBarManager), nameof(LoadingBarManager.ToggleLoadingBar))]
public static class LoadingPatch
{
    public static void Postfix(LoadingBarManager __instance)
    {
        var logo = __instance.loadingBar.transform.GetChild(1).GetChild(0);
        logo.GetComponent<Image>().sprite = TouAssets.BannerDark.LoadAsset();
        logo.localScale = new Vector3(1f, 1.2f, 1f);
    }
}