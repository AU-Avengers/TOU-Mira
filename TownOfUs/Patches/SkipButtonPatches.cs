using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class SkipButtonPatches
{
    private static NetworkedPlayerInfo? meetingTarget;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    public static void Prefix(NetworkedPlayerInfo target)
    {
        meetingTarget = target;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static void Postfix(MeetingHud __instance)
    {
        var genOpt = (SkipState)OptionGroupSingleton<VanillaTweakOptions>.Instance.SkipButtonDisable.Value;

        // Deactivate skip Button if skipping on emergency meetings is disabled
        if ((!meetingTarget && genOpt == SkipState.Emergency) || genOpt == SkipState.Always)
        {
            __instance.SkipVoteButton.gameObject.SetActive(false);
        }
    }
}