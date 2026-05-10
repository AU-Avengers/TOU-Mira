using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class MinerVentPatch
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
    [HarmonyPostfix]
    public static void VentSetButtonsPatch(Vent __instance, [HarmonyArgument(0)] bool enabled)
    {
        if (OptionGroupSingleton<MinerOptions>.Instance.MineVisibility == MineVisiblityOptions.Immediate)
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

        if (!enabled)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is MinerRole)
        {
            return;
        }

        if (!__instance.name.Contains("MinerVent"))
        {
            return;
        }

        Vent[] nearbyVents = __instance.NearbyVents;
        for (var i = 0; i < __instance.Buttons.Length; i++)
        {
            var buttonBehavior = __instance.Buttons[i];
            var vent = nearbyVents[i];

            if (vent != null && !vent.myRend.enabled)
            {
                buttonBehavior.gameObject.SetActive(false);
            }
        }
    }
}