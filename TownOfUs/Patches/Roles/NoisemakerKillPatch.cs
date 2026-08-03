using HarmonyLib;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(NoisemakerRole))]
public static class NoisemakerKillPatch
{
    // we run this code elsewhere!
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NoisemakerRole.NotifyOfDeath))]
    public static bool Prefix(NoisemakerRole __instance)
    {
        return false;
    }
}