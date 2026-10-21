using System.Runtime.CompilerServices;
using AchievementsAPI;
using AchievementsAPI.API;
using Reactor.Localization.Utilities;
using TownOfUs.Achievements;
using UnityEngine;

namespace TownOfUs.Modules;

/// <summary>
/// Typed bridge to AchievementsAPI. Every member of this type touches AchievementsAPI types,
/// so none of them may execute unless <see cref="AchievementsAPIHelper.IsLoaded"/> is true.
/// All entry points are marked <see cref="MethodImplOptions.NoInlining"/> so the JIT never pulls
/// AchievementsAPI types into a caller's frame when the plugin is absent; the bridge assembly
/// dependency is only resolved lazily, once a gated call actually runs.
/// </summary>
internal static class AchievementsAPIBridge
{
    private static readonly Dictionary<AchievementSpec, BaseAchievement> Achievements = new();

    /// <summary>
    /// Registers a mod's achievements tab (and all of its achievements) with AchievementsAPI.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterMod(AchievementTabSpec tab, IReadOnlyCollection<AchievementSpec> specs)
    {
        var tabName = tab.TabLocaleKey != null
            ? MiraLocaleManager.GetOrCreateLocaleString(tab.TabLocaleKey)
            : CustomStringName.CreateAndRegister(tab.TabName);

        var softTab = new SoftAchievementsTab(tabName, tab.TabColor, tab.TabIcon?.LoadAsset(), tab.CategoryId,
            tab.ModName, () => tab.ModIcon?.LoadAsset());
        AchievementsManager.RegisterTab(softTab);

        foreach (var spec in specs)
        {
            var achievement = Materialize(tab, spec);
            softTab.RegisterAchievement(achievement);
            Achievements[spec] = achievement;
        }
    }

    /// <summary>
    /// Supplies Town of Us artwork for the built-in API categories if no other mod already has.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterTouCategoryIcons()
    {
        AchievementsManager.GetCategory(AchievementCategories.General)?.SetDefaultIcon(() => TouRoleIcons.Mayor);
        AchievementsManager.GetCategory(AchievementCategories.Crewmate)?.SetDefaultIcon(() => TouRoleIcons.Crewmate);
        AchievementsManager.GetCategory(AchievementCategories.Impostor)?.SetDefaultIcon(() => TouRoleIcons.Impostor);
        AchievementsManager.GetCategory(AchievementCategories.Neutral)?.SetDefaultIcon(() => TouRoleIcons.Neutral);
    }

    /// <summary>
    /// Blocks achievement unlocks/progress while Town of Us' multiplayer freeplay is active.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterUnlockBlockers()
    {
        AchievementsManager.UnlockBlockers.Add(() => MultiplayerFreeplayMode.Enabled);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsUnlocked(AchievementSpec spec)
    {
        return Achievements.TryGetValue(spec, out var achievement) && achievement.Unlocked;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Unlock(AchievementSpec spec)
    {
        if (Achievements.TryGetValue(spec, out var achievement))
        {
            achievement.Unlock();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Reset(AchievementSpec spec)
    {
        if (Achievements.TryGetValue(spec, out var achievement))
        {
            achievement.Reset();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ResetAll(IReadOnlyCollection<AchievementSpec> specs)
    {
        foreach (var spec in specs)
        {
            if (Achievements.TryGetValue(spec, out var achievement))
            {
                achievement.Reset(save: false);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int GetCurrentValue(AchievementSpec spec)
    {
        return Achievements.TryGetValue(spec, out var achievement) && achievement is CountAchievement count
            ? count.CurrentValue : 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Increment(AchievementSpec spec, int amount, bool showOnUI)
    {
        if (Achievements.TryGetValue(spec, out var achievement) && achievement is CountAchievement count)
        {
            count.Increment(amount, showOnUI);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetValue(AchievementSpec spec, int value, bool showOnUI)
    {
        if (Achievements.TryGetValue(spec, out var achievement) && achievement is CountAchievement count)
        {
            count.SetValue(value, showOnUI);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsMenuOpen()
    {
        return AchievementsMenu.Instance != null;
    }

    private static BaseAchievement Materialize(AchievementTabSpec tab, AchievementSpec spec)
    {
        var name = Locale(tab, spec.Key, spec.Name, "Name");
        var description = Locale(tab, spec.Key, spec.Description, "Description");

        BaseAchievement achievement;
        if (spec.ProgressGoal > 0)
        {
            achievement = new RuntimeCountAchievement(name, description, 0, spec.ProgressGoal,
                MapPersistence(spec.Persistence), spec.Rarity, spec.Hidden, hideProgress: spec.HideProgress, assembly: tab.Assembly, tags: spec.Tags);
        }
        else
        {
            achievement = new RuntimeAchievement(name, description, spec.Rarity, spec.Hidden, assembly: tab.Assembly, tags: spec.Tags);
        }

        // Preserve previously shipped storage ids: "{AssemblyName}_{DisplayName}".
        achievement.Id = spec.Id ?? $"{tab.Assembly.GetName().Name}_{spec.Name}";
        achievement.Category = spec.Category;
        if (spec.ToastOnGameEnd)
        {
            achievement.ToastTiming = AchToastTiming.OnGameEnd;
        }

        ApplyArt(achievement, spec, tab.Art);
        return achievement;
    }

    private static StringNames Locale(AchievementTabSpec tab, string key, string fallback, string part)
    {
        return tab.LocalePrefix != null
            ? MiraLocaleManager.GetOrCreateLocaleString($"{tab.LocalePrefix}.{key}.{part}")
            : CustomStringName.CreateAndRegister(fallback);
    }

    private static AchPersistence MapPersistence(AchievementPersistence persistence)
    {
        return persistence switch
        {
            AchievementPersistence.ThroughoutRounds => AchPersistence.ThroughoutRounds,
            AchievementPersistence.ResetOnRoundStart => AchPersistence.ResetOnRoundStart,
            AchievementPersistence.ResetOnMeetingStart => AchPersistence.ResetOnMeetingStart,
            _ => AchPersistence.ThroughoutSessions
        };
    }

    private static void ApplyArt(BaseAchievement achievement, AchievementSpec spec, AchievementArt? art)
    {
        Func<Sprite?> iconProvider = () => spec.Icon;
        var subIconProvider = spec.SubIcon != null ? () => spec.SubIcon : (Func<Sprite?>?)null;
        var menuBgProvider = art?.MenuBg != null ? () => art.MenuBg : (Func<Sprite?>?)null;
        var toastBgProvider = art?.ToastBg != null ? () => art.ToastBg : (Func<Sprite?>?)null;
        var tooltip = spec.SubIconTooltip;
        var subIconTooltipProvider = tooltip != null
            ? (Func<string?>)(() => MiraLocaleManager.Get(tooltip, tooltip))
            : null;

        if (achievement is RuntimeAchievement runtime)
        {
            runtime.IconProvider = iconProvider;
            runtime.MenuBgProvider = menuBgProvider;
            runtime.ToastBgProvider = toastBgProvider!;
            runtime.MenuSubIconProvider = subIconProvider;
            runtime.MenuSubIconTooltipProvider = subIconTooltipProvider;
            runtime.MenuIconOffsetOverride = art?.MenuIconOffset;
            runtime.MenuTitleOffsetOverride = art?.MenuTitleOffset;
            runtime.MenuDescOffsetOverride = art?.MenuDescOffset;
            runtime.MenuSubIconOffsetOverride = art?.MenuSubIconOffset;
            runtime.MenuSubIconScaleOverride = art?.MenuSubIconScale;
            runtime.ToastIconOffsetOverride = art?.ToastIconOffset;
        }
        else if (achievement is RuntimeCountAchievement count)
        {
            count.IconProvider = iconProvider;
            count.MenuBgProvider = menuBgProvider;
            count.ToastBgProvider = toastBgProvider;
            count.MenuSubIconProvider = subIconProvider;
            count.MenuSubIconTooltipProvider = subIconTooltipProvider;
            count.MenuIconOffsetOverride = art?.MenuIconOffset;
            count.MenuTitleOffsetOverride = art?.MenuTitleOffset;
            count.MenuDescOffsetOverride = art?.MenuDescOffset;
            count.MenuSubIconOffsetOverride = art?.MenuSubIconOffset;
            count.MenuSubIconScaleOverride = art?.MenuSubIconScale;
            count.ToastIconOffsetOverride = art?.ToastIconOffset;
        }
    }
}