using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP;
using TownOfUs.Achievements;

namespace TownOfUs.Modules;

/// <summary>
/// Optional integration with the AchievementsAPI plugin. This type contains no references to
/// AchievementsAPI, so it is safe to use anywhere; when the plugin is absent every method is a
/// no-op. Mods that want to hook into Town of Us's achievements integration can register their
/// own achievements with <see cref="RegisterMod"/> and trigger them with the methods below,
/// without taking a dependency on AchievementsAPI themselves.
/// </summary>
/// <remarks>
/// The typed calls live in <c>AchievementsAPIBridge</c>, which is only ever invoked — and thus
/// only JIT-compiled — once <see cref="IsLoaded"/> is known to be true.
/// </remarks>
public static class AchievementsAPIHelper
{
    /// <summary>
    /// The BepInEx GUID of the AchievementsAPI plugin.
    /// </summary>
    public const string PluginGuid = "achievementsapi";

    /// <summary>
    /// True when the AchievementsAPI plugin is loaded and achievements can be registered/triggered.
    /// </summary>
    public static bool IsLoaded =>
        IL2CPPChainloader.Instance != null && IL2CPPChainloader.Instance.Plugins.ContainsKey(PluginGuid);

    private static readonly HashSet<AchievementSpec> Tentative = new();

    /// <summary>
    /// Registers Town of Us's achievement tabs and all of its achievements. Called once during
    /// plugin load; does nothing when AchievementsAPI is not installed.
    /// </summary>
    public static void RegisterTouAchievements()
    {
        if (!IsLoaded)
        {
            return;
        }

        AchievementsAPIBridge.RegisterMod(TouAchievementGroups.General, TouAchievements.General);
        AchievementsAPIBridge.RegisterMod(TouAchievementGroups.Crew, TouAchievements.Crew);
        AchievementsAPIBridge.RegisterMod(TouAchievementGroups.Imp, TouAchievements.Imp);
        AchievementsAPIBridge.RegisterMod(TouAchievementGroups.Neut, TouAchievements.Neut);
        AchievementsAPIBridge.RegisterTouCategoryIcons();
    }

    /// <summary>
    /// Registers another mod's achievements tab and all of its achievements. Call from the mod's
    /// plugin Load method; does nothing when AchievementsAPI is not installed.
    /// </summary>
    /// <param name="tab">Description of the tab to register the achievements under.</param>
    /// <param name="achievements">The achievements to register.</param>
    public static void RegisterMod(AchievementTabSpec tab, IReadOnlyCollection<AchievementSpec> achievements)
    {
        if (!IsLoaded)
        {
            return;
        }

        AchievementsAPIBridge.RegisterMod(tab, achievements);
    }

    /// <summary>
    /// Unlocks an achievement. No-op if it isn't registered or the API is missing.
    /// </summary>
    public static void Unlock(AchievementSpec achievement)
    {
        if (!IsLoaded)
        {
            return;
        }

        AchievementsAPIBridge.Unlock(achievement);
    }

    /// <summary>
    /// Returns whether an achievement is currently unlocked (false when the API is missing).
    /// </summary>
    public static bool IsUnlocked(AchievementSpec achievement)
    {
        return IsLoaded && AchievementsAPIBridge.IsUnlocked(achievement);
    }

    /// <summary>
    /// Unlocks an achievement tentatively: it unlocks now, but stays tracked in the tentative set
    /// so a Time Lord rewind or a host abort can roll it back with <see cref="ResetTentative"/>.
    /// Call <see cref="ClearTentative"/> at round/meeting boundaries to make them permanent.
    /// </summary>
    public static void UnlockTentative(AchievementSpec achievement)
    {
        if (!IsLoaded || IsUnlocked(achievement))
        {
            return;
        }

        AchievementsAPIBridge.Unlock(achievement);
        Tentative.Add(achievement);
    }

    /// <summary>
    /// Clears the tentative tracking set without resetting achievements. Call at round and meeting
    /// boundaries so previously tentative unlocks are no longer rolled back.
    /// </summary>
    public static void ClearTentative()
    {
        Tentative.Clear();
    }

    /// <summary>
    /// Resets all achievements unlocked tentatively this round.
    /// </summary>
    public static void ResetTentative()
    {
        if (Tentative.Count == 0)
        {
            return;
        }

        if (IsLoaded)
        {
            AchievementsAPIBridge.ResetAll(Tentative);
        }

        Tentative.Clear();
    }

    /// <summary>
    /// Returns the current progress value of a count achievement (0 when unavailable).
    /// </summary>
    public static int GetCurrentValue(AchievementSpec achievement)
    {
        return IsLoaded ? AchievementsAPIBridge.GetCurrentValue(achievement) : 0;
    }

    /// <summary>
    /// Increments a count achievement's progress.
    /// </summary>
    /// <param name="achievement">The achievement to advance.</param>
    /// <param name="amount">Amount to increment by.</param>
    /// <param name="showOnUI">Whether to show a toast/progress popup.</param>
    public static void Increment(AchievementSpec achievement, int amount = 1, bool showOnUI = true)
    {
        if (!IsLoaded)
        {
            return;
        }

        AchievementsAPIBridge.Increment(achievement, amount, showOnUI);
    }

    /// <summary>
    /// Sets a count achievement's progress value directly.
    /// </summary>
    public static void SetValue(AchievementSpec achievement, int value, bool showOnUI = true)
    {
        if (!IsLoaded)
        {
            return;
        }

        AchievementsAPIBridge.SetValue(achievement, value, showOnUI);
    }

    /// <summary>
    /// Resets an achievement back to its locked, unprogressed state.
    /// </summary>
    public static void Reset(AchievementSpec achievement)
    {
        if (!IsLoaded)
        {
            return;
        }

        AchievementsAPIBridge.Reset(achievement);
    }

    /// <summary>
    /// Whether the AchievementsAPI menu is currently open (false when the API is missing).
    /// </summary>
    public static bool HasOpenMenu()
    {
        return IsLoaded && AchievementsAPIBridge.IsMenuOpen();
    }
}
