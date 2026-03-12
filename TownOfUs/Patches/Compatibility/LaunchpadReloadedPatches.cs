using HarmonyLib;
using Il2CppInterop.Runtime;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;

namespace TownOfUs.Patches.Compatibility;

[HarmonyPatch]
public static class LaunchpadReloadedPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void StartPatch(PlayerControl __instance)
    {
        if (ModCompatibility.LaunchpadLoaded &&
            __instance.TryGetComponent(Il2CppType.From(ModCompatibility.LaunchpadTagManager), out var tagManager))
        {
            tagManager.Destroy();
        }
    }

    /*
    [HarmonyPatch(typeof(CustomRoleUtils), nameof(CustomRoleUtils.CanSpawnOnCurrentMode))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    public static bool CanSpawnOnCurrentModePatch(RoleBehaviour role, ref bool __result)
    {
        if (role is ITownOfUsRole)
        {
            return true;
        }

        if (ModCompatibility.LaunchpadLoaded && role is ICustomRole custom &&
            custom.ParentMod.MiraPlugin.OptionsTitleText is "Launchpad")
        {
            switch (custom.RoleName)
            {
                case "Executioner" or "Jester" or "Sheriff" or "Burrower":
                    __result = false;
                    return false;
            }
        }

        return true;
    }*/

    /*
    [HarmonyPatch(typeof(Helpers), nameof(Helpers.GetRoleName))]
    [HarmonyPrefix]
    public static bool GetRoleNamePatch(RoleBehaviour role, ref string __result)
    {
        if (role is ITownOfUsRole)
        {
            return true;
        }

        if (ModCompatibility.LaunchpadLoaded && role is ICustomRole custom &&
            custom.ParentMod.MiraPlugin.OptionsTitleText is "Launchpad")
        {
            switch (custom.RoleName)
            {
                case "Detective":
                    __result = "Sherlock";
                    return false;
                case "Medic":
                    __result = "Practitioner";
                    return false;
            }
        }

        return true;
    }*/
}
