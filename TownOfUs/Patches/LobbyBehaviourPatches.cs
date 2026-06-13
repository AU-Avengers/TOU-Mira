using HarmonyLib;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Patches.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class LobbyBehaviourPatches
{
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPatch(typeof(TutorialManager), nameof(TutorialManager.Awake))]
    [HarmonyPostfix]
    public static void LobbyStartPatch()
    {
        HaunterRole.ResetReveals();
        GameTimerPatch.ResetTimer();
        foreach (var role in GameHistory.AllRoles)
        {
            if (!role || role is not ITownOfUsRole touRole)
            {
                continue;
            }

            touRole.LobbyStart();
        }

        TeamChatPatches.CleanUpChats();
        GameHistory.ClearAll();
        ScreenFlash.Clear();
        MeetingMenu.ClearAll();
        EgotistModifier.CooldownReduction = 0f;
        EgotistModifier.SpeedMultiplier = 1f;
        UpCommandRequests.Clear();

        // Clear Time Lord snapshot data to prevent stale positions from previous games
        TimeLordRewindSystem.Reset();
        MiscUtils.ClearGarbageCollector();
    }
}