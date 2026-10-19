using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class VentPatches
{
    public static bool InVision(PlayerControl player)
    {
        return InVision(player, PlayerControl.LocalPlayer);
    }

    public static bool InVision(PlayerControl player, PlayerControl observer)
    {
        var observerPosition = observer.GetTruePosition();
        var vector = player.GetTruePosition() - observerPosition;
        var magnitude = vector.magnitude;

        if (magnitude < PlayerControl.LocalPlayer.lightSource.viewDistance &&
            !PhysicsHelpers.AnyNonTriggersBetween(observerPosition, vector.normalized, magnitude,
                Constants.ShipAndObjectsMask))
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    [HarmonyPrefix]
    public static bool EnterVentPatch(Vent __instance, PlayerControl pc)
    {
        if (!__instance.EnterVentAnim)
        {
            return true;
        }

        if (!OptionGroupSingleton<VanillaTweakOptions>.Instance.HideVentAnimationNotInVision.Value)
        {
            return true;
        }

        if (InVision(pc))
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    [HarmonyPrefix]
    public static bool ExitVentPatch(Vent __instance, PlayerControl pc)
    {
        if (!__instance.ExitVentAnim)
        {
            return true;
        }

        if (!OptionGroupSingleton<VanillaTweakOptions>.Instance.HideVentAnimationNotInVision.Value)
        {
            return true;
        }

        if (InVision(pc))
        {
            return true;
        }

        return false;
    }
}