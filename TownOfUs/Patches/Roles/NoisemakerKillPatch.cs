using HarmonyLib;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(NoisemakerRole))]
public static class NoisemakerKillPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NoisemakerRole.NotifyOfDeath))]
    public static bool Prefix(NoisemakerRole __instance)
    {
        var scBody = MiscUtils.GetFakePlayer(__instance.Player);
        if (scBody != null)
        {
            return false;
        }
        return true;
    }
}