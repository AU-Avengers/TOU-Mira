using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
// using TownOfUs.Modifiers.Crewmate;
// using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
// using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetPlayerSpeedMod))]
public static class PlayerSpeedPatch
{
    // ReSharper disable once InconsistentNaming
    public static void Postfix(PlayerControl pc, ref float __result)
    {
        __result *= pc.GetAppearance().Speed;


        // if (OptionGroupSingleton<BarkeeperOptions>.Instance.Hangover && pc.HasModifier<BarkeeperRoleblockedModifier>() || OptionGroupSingleton<BootleggerOptions>.Instance.Hangover && pc.HasModifier<BootleggerRoleblockedModifier>())
        // {
        //     __result *= -1;
        // }

        if (pc.HasModifier<VenererSprintModifier>())
        {
            __result *= OptionGroupSingleton<VenererOptions>.Instance.NumSprintSpeed;
        }

        if (pc.TryGetModifier<VenererFreezeModifier>(out var freeze))
        {
            __result *= freeze.SpeedFactor;
        }
    }
}