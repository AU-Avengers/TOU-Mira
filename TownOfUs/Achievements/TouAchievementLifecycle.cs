using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using TownOfUs.Events.TouEvents;
using TownOfUs.GameOver;

namespace TownOfUs.Achievements;

/// <summary>
/// Tracks hidden-information achievements that unlock immediately (with a deferred toast)
/// but may need to be rolled back if the game state that earned them is undone.
/// </summary>
public static class TouAchievementLifecycle
{
    /// <summary>
    /// Unlocks an achievement tentatively: it unlocks now, but a Time Lord rewind or a
    /// host abort before the round/meeting boundary will reset it.
    /// </summary>
    public static void UnlockTentative(AchievementSpec achievement)
    {
        AchievementsAPIHelper.UnlockTentative(achievement);
    }

    /// <summary>
    /// Resets all tentative unlocks.
    /// </summary>
    public static void ResetTentative()
    {
        AchievementsAPIHelper.ResetTentative();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        AchievementsAPIHelper.ClearTentative();
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        AchievementsAPIHelper.ClearTentative();
    }

    /// <summary>
    /// A rewind can undo whatever earned a tentative achievement, so roll it back.
    /// </summary>
    [RegisterEvent]
    public static void TouAbilityEventHandler(TouAbilityEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        if (@event.AbilityType == AbilityType.TimeLordRewind)
        {
            ResetTentative();
        }
    }

    /// <summary>
    /// A host abort means the game never really concluded, so tentative unlocks are rolled back.
    /// </summary>
    [RegisterEvent]
    public static void BeforeGameEndEventHandler(BeforeGameEndEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        if (CustomGameOver.Instance is HostGameOver)
        {
            ResetTentative();
        }
    }
}