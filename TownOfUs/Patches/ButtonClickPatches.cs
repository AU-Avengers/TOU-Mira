using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class ButtonClickPatches
{
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPatch(typeof(PetButton), nameof(PetButton.DoClick))]
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool VanillaButtonChecks(ActionButton __instance)
    {
        // During Time Lord rewind, block ALL vanilla interactions (kill/report/use/pet/ability/sabotage/vent).
        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

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

        if (PlayerControl.LocalPlayer != null)
        {
            if (__instance is UseButton)
            {
                var localRole = PlayerControl.LocalPlayer.Data?.Role;
                if (localRole is PuppeteerRole puppeteerRole && puppeteerRole.Controlled != null)
                {
                    return true;
                }
                if (localRole is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
                {
                    return true;
                }
            }

            if (__instance is ReportButton)
            {
                if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanReport))
                {
                    return false;
                }
            }
            else
            {
                if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
                {
                    return false;
                }
            }
        }

        return true;
    }
}