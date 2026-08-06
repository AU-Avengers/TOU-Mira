using HarmonyLib;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class JesterNoVentMovePatch
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
    [HarmonyPrefix]
    public static bool JesterEnterVent()
    {
        if (!PlayerControl.LocalPlayer)
        {
            return true;
        }

        if (!PlayerControl.LocalPlayer.Data)
        {
            return true;
        }

        if (PlayerControl.LocalPlayer.Data.Role is JesterRole)
        {
            return false;
        }

        return true;
    }
}