using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Modules;

namespace TownOfUs.Events.Misc;

public static class MultiplayerFreeplayEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        MultiplayerFreeplayDebugState.CaptureBaselineIfNeeded();
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent _)
    {
        MultiplayerFreeplayDebugState.ResetCapturedBaseline();
    }
}
