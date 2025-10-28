using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class GhostRoleSabotagePatches
{
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static bool GhostRoleSabotageMinigamePatch(Minigame __instance)
    {
        var flag = __instance is AuthGame || __instance is AirshipAuthGame || __instance is TuneRadioMinigame || // Comms Minigames
                   __instance is ReactorMinigame || // Reactor Minigame
                   __instance is KeypadGame || // Oxygen Minigame
                   __instance is SwitchMinigame; // Lights Minigame

        if (flag && PlayerControl.LocalPlayer.Data.Role is IGhostRole)
        {
            try
            {
                Minigame.Instance.Close();
                Minigame.Instance.Close();
            }
            catch
            {
                // ignored
            }
            return false;
        }

        return true;
    }
}