using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Modules;

namespace TownOfUs.Events.Misc;

public static class FreeplayDebugEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        if (!TutorialManager.InstanceExists)
        {
            return;
        }

        FreeplayDebugState.CaptureBaselineIfNeeded();
    }
}


