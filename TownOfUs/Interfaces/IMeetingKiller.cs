namespace TownOfUs.Interfaces;

public interface IMeetingKiller
{
    void TriggerMeetingAnimation(PlayerControl source, PlayerControl target, PlayerVoteArea targetVoteArea, int associatedAnimKey = -1);
}