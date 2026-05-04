using HarmonyLib;
using TownOfUs.Modules.Cosmetics;

namespace TownOfUs.Patches.Cosmetics;

[HarmonyPatch(typeof(ReferenceDataManager._Initialize_d__7), "MoveNext")]
public static class InstallCosmeticsPatch
{
    private static bool _didRun = false;

    public static void Postfix(ReferenceDataManager._Initialize_d__7 __instance)
    {
        if (__instance.__1__state >= 0 || _didRun)
        {
            // only run after the original method has fully completed
            return;
        }

        Info("Loading cosmetics...");
        CosmeticsLoader.Instance.LoadCosmetics();
        Info("Cosmetics loaded");

        Info("Patching HatManager to include custom cosmetics");
        CosmeticsLoader.Instance.InstallCosmetics(__instance.__4__this.Refdata);
        Info("Loaded custom cosmetics into HatManager");

        // second guard to prevent double execution
        _didRun = true;
    }
}