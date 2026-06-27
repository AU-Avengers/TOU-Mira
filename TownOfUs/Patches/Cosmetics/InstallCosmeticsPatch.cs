using System.Collections;
using HarmonyLib;
using MiraAPI.Utilities;
using TownOfUs.Modules.Cosmetics;

namespace TownOfUs.Patches.Cosmetics;

[HarmonyPatch(typeof(ReferenceDataManager), nameof(ReferenceDataManager.Initialize))]
public static class InstallCosmeticsPatch
{
    private static bool _didRun;

    public static void Postfix(ReferenceDataManager __instance, ref IEnumerator __result)
    {
        if (_didRun)
        {
            return;
        }
        __result = Helpers.CreateWrapper(__result, () => 
        {
            Info("Loading cosmetics...");
            CosmeticsLoader.Instance.LoadCosmetics();
            Info("Cosmetics loaded");

            Info("Patching HatManager to include custom cosmetics");
            CosmeticsLoader.Instance.InstallCosmetics(__instance.Refdata);
            Info("Loaded custom cosmetics into HatManager");

            // second guard to prevent double execution
            _didRun = true;
        });
    }
}