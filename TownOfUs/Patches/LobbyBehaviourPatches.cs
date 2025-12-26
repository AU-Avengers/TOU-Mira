using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;

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
        UpCommandRequests.Clear();
        ParasiteControlState.ClearAll();

        // Clear Time Lord snapshot data to prevent stale positions from previous games
        TimeLordRewindSystem.Reset();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.TryGetModifier<ParasiteInfectedModifier>(out var mod))
            {
                player.RemoveModifier(mod);
            }
        }
    }
}