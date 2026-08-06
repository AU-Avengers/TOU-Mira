namespace TownOfUs.Interfaces;

public interface IRewindImmune
{
    bool IgnoredByRewind { get; }
    bool IgnoredByRecording { get; }
}