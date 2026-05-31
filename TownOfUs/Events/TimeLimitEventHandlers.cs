using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Patches;

namespace TownOfUs.Events;

public static class TimeLimitEventHandlers
{
    [RegisterEvent]
    public static void GameStartEventHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when round starts.
        }

        if (TutorialManager.InstanceExists)
        {
            return; // Shouldn't run in Freeplay
        }

        if (!OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled || MiscUtils.CurrentGamemode() is not TouGamemode.Classic)
        {
            return;
        }

        // begin timer
        GameTimerPatch.BeginTimer();
    }
}