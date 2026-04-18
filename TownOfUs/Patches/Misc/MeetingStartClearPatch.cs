using HarmonyLib;

namespace TownOfUs.Patches.Misc;

/// <summary>
/// Clears FakeChatHistory when voting completes (meeting is ending) so
/// /info only replays messages from the most recent meeting.
/// We clear at VotingComplete rather than MeetingHud.Start because roles
/// like Doomsayer and Inquisitor call AddFakeChat during OnMeetingStart —
/// clearing at Start would wipe those before they get recorded.
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class MeetingStartClearPatch
{
    public static void Prefix()
    {
        FakeChatHistory.Clear();
    }
}