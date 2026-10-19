using System.Collections;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Achievements;

/// <summary>
/// Rolls the hidden "Go buy a lottery ticket" achievement once on startup and then every 30 minutes.
/// </summary>
public static class LotteryAchievementRoller
{
    private const long Odds = 292_000_000;
    private const float IntervalSeconds = 30f * 60f;

    private static bool _started;

    public static void Start()
    {
        if (_started || !AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        _started = true;
        Coroutines.Start(CoRoll());
    }

    private static IEnumerator CoRoll()
    {
        while (true)
        {
            if (AchievementsAPIHelper.IsUnlocked(TouAchievements.GoBuyALotteryTicket))
            {
                yield break;
            }

            if (System.Random.Shared.NextInt64(Odds) == 0)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.GoBuyALotteryTicket, needsToMeetRequirements: false);
                yield break;
            }

            yield return new WaitForSeconds(IntervalSeconds);
        }
    }
}
