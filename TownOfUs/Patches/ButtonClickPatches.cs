using HarmonyLib;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class ButtonClickPatches
{
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPatch(typeof(PetButton), nameof(PetButton.DoClick))]
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool VanillaButtonChecks(ActionButton __instance)
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        return true;
    }
}