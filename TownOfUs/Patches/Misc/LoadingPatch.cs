using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(AmongUsLoadingBar), nameof(AmongUsLoadingBar.OnEnable))]
public static class LoadingPatch
{
    public static void Postfix(AmongUsLoadingBar __instance)
    {
        var logo = __instance.transform.GetChild(1).GetChild(0);
        logo.GetComponent<Image>().sprite = TouAssets.BannerDark;
        logo.localScale = new Vector3(1f, 1.2f, 1f);
    }
}