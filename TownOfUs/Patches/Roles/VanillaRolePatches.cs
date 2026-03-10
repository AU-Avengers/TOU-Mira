using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(RoleBehaviour))]
public static class VanillaRolePatches
{
    [HarmonyPatch(nameof(RoleBehaviour.TeamColor), MethodType.Getter)]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CrewmateColorPrefix(RoleBehaviour __instance, ref Color __result)
    {
        if (!__instance.IsCrewmate())
        {
            return true;
        }

        if (TownOfUsColors.UseBasic)
        {
            __result = Palette.CrewmateBlue;
            return false;
        }

        if (!__instance.IsCustomRole())
        {
            var newColor = __instance.Role switch
            {
                RoleTypes.GuardianAngel => TownOfUsColors.GuardianAngel,
                RoleTypes.Detective => TownOfUsColors.Detective,
                RoleTypes.Tracker => TownOfUsColors.Tracker,
                RoleTypes.Scientist => TownOfUsColors.Scientist,
                RoleTypes.Noisemaker => TownOfUsColors.Noisemaker,
                _ => Palette.CrewmateBlue
            };
            __result = newColor;
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(RoleBehaviour.RoleIconSolid), MethodType.Getter)]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool VanillaIconPrefix(RoleBehaviour __instance, ref Sprite __result)
    {
        if (__instance.IsCustomRole())
        {
            return true;
        }

        var newIcon = TouRoleUtils.TryGetVanillaRoleIcon(__instance.Role);
        if (newIcon != null)
        {
            // Error($"Patched role icon for {__instance.Role}");
            __result = newIcon;
            return false;
        }
        return true;
    }
}