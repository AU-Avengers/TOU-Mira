using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using TownOfUs.Events.TouEvents;
using TownOfUs.GameOver;
using TownOfUs.Modules;

namespace TownOfUs.Achievements;

/// <summary>
/// Tracks hidden-information achievements that unlock immediately (with a deferred toast)
/// but may need to be rolled back if the game state that earned them is undone.
/// </summary>
public static class TouAchievementLifecycle
{
    private static readonly HashSet<BaseAchievement> TentativeThisRound = new();

    /// <summary>
    /// Unlocks an achievement tentatively: it unlocks now, but a Time Lord rewind or a
    /// host abort before the round/meeting boundary will reset it.
    /// </summary>
    public static void UnlockTentative(BaseAchievement achievement)
    {
        if (!AchievementsAPIHelper.IsLoaded || achievement.Unlocked)
        {
            return;
        }

        achievement.Unlock();
        TentativeThisRound.Add(achievement);
    }

    private static void ResetTentative()
    {
        foreach (var achievement in TentativeThisRound)
        {
            achievement.Reset();
        }

        TentativeThisRound.Clear();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        TentativeThisRound.Clear();
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        TentativeThisRound.Clear();
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
