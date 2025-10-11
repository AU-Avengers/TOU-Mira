using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modifiers.Neutral;
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


        if (pc.HasModifier<VenererSprintModifier>())
        {
            __result *= OptionGroupSingleton<VenererOptions>.Instance.NumSprintSpeed;
        }

        if (pc.TryGetModifier<VenererFreezeModifier>(out var freeze))
        {
            __result *= freeze.SpeedFactor;
        }

        if (pc.TryGetModifier<DragModifier>(out var drag))
        {
            __result *= drag.SpeedFactor;
        }

        if (pc.TryGetModifier<ChefServedModifier>(out var served) && served.TimerActive)
        {
            __result *= served.SpeedFactor;
        }
    }
}