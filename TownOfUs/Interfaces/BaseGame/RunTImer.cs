
namespace TownOfUs.Interfaces.BaseGame;

// ReSharper disable once InconsistentNaming
public sealed partial class BaseGame
{
    public interface RunTimer
    {
        float GetTimer(SystemTypes system);
    }
}