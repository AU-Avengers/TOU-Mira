using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class GlitchPatches
{
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool DisabledReportButtonPatch(ActionButton __instance)
    {
        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanReport))
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>())
        {
            PlayerControl.LocalPlayer.GetModifier<GlitchHackedModifier>()!.ShowHacked();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool GlitchHackedSabotageButtonPatch(ActionButton __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>())
        {
            PlayerControl.LocalPlayer.GetModifier<GlitchHackedModifier>()!.ShowHacked();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
    [HarmonyPrefix]
    public static bool GlitchHackedToggleMapVisiblePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() &&
            !PlayerControl.LocalPlayer.GetModifier<GlitchHackedModifier>()!.ShouldHideHacked)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return true;
    }
}