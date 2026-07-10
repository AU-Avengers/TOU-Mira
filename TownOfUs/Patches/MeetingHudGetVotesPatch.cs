using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingHudGetVotesPatch
{
    public static MeetingHud.VoterState[] States { get; private set; } = [];

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    public static void VotingCompletePrefix(Il2CppStructArray<MeetingHud.VoterState> states)
    {
        // CODE REVIEW 22/2/2025 AEDT (D/M/Y)
        // ---------------------------------
        // Why?
        States = states;
        // 4/4/2025 - XtraCube
        // Caching the states lets us use voter state to make haunt menu for Jester after exile.
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.OnDestroy))]
    public static void OnDestroyPostfix()
    {
        States = [];
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void StartPostfix()
    {
        States = [];
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.PopulateResults))]
    public static void PopulatePrefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
    {
        var swapper = CustomRoleUtils.GetActiveRolesOfType<SwapperRole>().FirstOrDefault(swapper => !swapper.Player.HasDied() && swapper.Swap1 && swapper.Swap2);
        if (swapper == null)
        {}
    }
}