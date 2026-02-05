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
    public static bool CanUseAbilities()
    {
        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPatch(typeof(PetButton), nameof(PetButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool VanillaButtonChecks(ActionButton __instance)
    {
        if (!CanUseAbilities())
        {
            return false;
        }

        if (PlayerControl.LocalPlayer != null)
        {
            var disabledMods = PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>();
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

            if (__instance is ReportButton && disabledMods.Any(x => !x.CanReport))
            {
                return false;
            }

            if (disabledMods.Any(x => !x.CanUseConsoles))
            {
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool AbilityButtonChecks(ActionButton __instance)
    {
        if (!CanUseAbilities())
        {
            return false;
        }

        // During Time Lord rewind, block ALL vanilla interactions (kill/report/use/pet/ability/sabotage/vent).
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

        if (PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return true;
    }
}
