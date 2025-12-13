using AmongUs.GameOptions;
using HarmonyLib;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class ButtonClickPatches
{
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPatch(typeof(PetButton), nameof(PetButton.DoClick))]
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool VanillaButtonChecks(ActionButton __instance)
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return false;
        }

        if (MeetingHud.Instance)
        {
            if (__instance is AbilityButton &&
                PlayerControl.LocalPlayer != null &&
                PlayerControl.LocalPlayer.Data?.Role != null &&
                PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Detective)
            {
                return true;
            }

            return false;
        }

        return true;
    }
}