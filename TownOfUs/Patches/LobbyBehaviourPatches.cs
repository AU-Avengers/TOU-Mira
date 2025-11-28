using HarmonyLib;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Roles;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class LobbyBehaviourPatches
{
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void LobbyStartPatch(LobbyBehaviour __instance)
    {
        foreach (var role in GameHistory.AllRoles)
        {
            if (!role || role is not ITownOfUsRole touRole)
            {
                continue;
            }

            touRole.LobbyStart();
        }

        GameHistory.ClearAll();
        ScreenFlash.Clear();
        MeetingMenu.ClearAll();
        EgotistModifier.CooldownReduction = 0f;
        EgotistModifier.SpeedMultiplier = 1f;
    }
}