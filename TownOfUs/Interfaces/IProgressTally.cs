namespace TownOfUs.Interfaces;

public interface IProgressTally
{
    bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress);
    string ProgressOnSummaryNormal { get; }
    string ProgressOnSummaryDetailed { get; }
    TallyLocation TallyPlacement(bool inMeeting) => TallyLocation.Auto;
}

public enum TallyLocation
{
    Auto,
    PlayerName,
    RoleName,
    AboveName,
    BelowName
}