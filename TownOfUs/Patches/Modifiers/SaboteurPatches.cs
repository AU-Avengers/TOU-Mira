using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Options.Modifiers.Impostor;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class SaboteurPatches
{
    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
    [HarmonyPrefix]
    public static void SabotageSystemUpdate(SabotageSystemType __instance, ref PlayerControl player)
    {
        if (PlayerControl.AllPlayerControls.Count <= 1)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        if (!player.HasModifier<SaboteurModifier>())
        {
            return;
        }

        var options = OptionGroupSingleton<SaboteurOptions>.Instance;

        if (__instance.Timer <= options.ReducedSaboCooldown)
        {
            __instance.Timer = 0f;
        }
    }

    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
    [HarmonyPrefix]
    public static void SabotageSystemDeserializePrefix(SabotageSystemType __instance)
    {
        if (PlayerControl.AllPlayerControls.Count <= 1)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        if (__instance.AnyActive)
        {
            return;
        }

        if (__instance.initialCooldown)
        {
            return;
        }

        foreach (var sab in ModifierUtils.GetActiveModifiers<SaboteurModifier>())
        {
            sab.Timer = __instance.Timer;
        }
    }

    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
    [HarmonyPostfix]
    public static void SabotageSystemDeserializePostfix(SabotageSystemType __instance)
    {
        if (PlayerControl.AllPlayerControls.Count <= 1)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        if (__instance.AnyActive)
        {
            return;
        }

        if (__instance.initialCooldown)
        {
            return;
        }

        foreach (var sab in ModifierUtils.GetActiveModifiers<SaboteurModifier>())
        {
            __instance.Timer = sab.Timer;
        }
    }
}