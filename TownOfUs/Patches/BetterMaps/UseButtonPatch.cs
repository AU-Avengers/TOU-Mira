using HarmonyLib;
using MiraAPI.Roles;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.BetterMaps;

[HarmonyPatch]
public static class UseButtonPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    public static bool UseButtonClickPatch(UseButton __instance)
    {
        if (PlayerControl.LocalPlayer.HasDied() || !PlayerControl.LocalPlayer.Is(ModdedRoleTeams.Crewmate) || __instance.currentTarget == null) return true;
        var icon = __instance.currentTarget.UseIcon;
        if (icon is ImageNames.AdminMapButton || icon is ImageNames.MIRAAdminButton || icon is ImageNames.PolusAdminButton || icon is ImageNames.AirshipAdminButton)
        {
            return MiscUtils.CanUseUtility(GameUtility.Admin);
        }
        if (icon is ImageNames.CamsButton)
        {
            return MiscUtils.CanUseUtility(GameUtility.Cams);
        }
        if (icon is ImageNames.DoorLogsButton)
        {
            return MiscUtils.CanUseUtility(GameUtility.Doorlog);
        }
        if (icon is ImageNames.VitalsButton)
        {
            return MiscUtils.CanUseUtility(GameUtility.Vitals);
        }
        return true;
    }
}