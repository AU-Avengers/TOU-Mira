using HarmonyLib;

namespace TownOfUs.Patches.Misc;

/// <summary>
/// Clears <see cref="FakeChatHistory"/> when voting completes (meeting is ending) so
/// /info only replays messages from the most recent meeting.
/// <para/>
/// We clear at <see cref="MeetingHud.VotingComplete"/> rather than <see cref="MeetingHud.Start"/> because roles
/// like <see cref="TownOfUs.Roles.Neutral.DoomsayerRole"/> and <see cref="TownOfUs.Roles.Neutral.InquisitorRole"/>
/// call <see cref="MiscUtils.AddFakeChat"/> during <see cref="RoleBehaviour.OnMeetingStart"/> —
/// clearing at <see cref="MeetingHud.Start"/> would wipe those before they get recorded.
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class MeetingStartClearPatch
{
    public static void Prefix()
    {
        FakeChatHistory.Clear();
    }
}