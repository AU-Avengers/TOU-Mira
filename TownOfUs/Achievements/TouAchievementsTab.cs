using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

/// <summary>
/// Base for all Town of Us achievement tabs. Each tab feeds one of the shared API categories
/// (General / Crewmate / Impostor / Neutral) so other mods can contribute to the same categories;
/// the menu groups entries inside a category under this mod's name and icon.
/// </summary>
public abstract class TouAchievementsTab : AchievementsTab
{
    /// <inheritdoc/>
    public override string ModName => "Town of Us";

    /// <inheritdoc/>
    public override Sprite ModIcon => TouAssets.TouMiraIcon.LoadAsset();

    /// <summary>
    /// Supplies Town of Us artwork for the built-in categories if no other mod already has.
    /// </summary>
    public static void RegisterCategoryIcons()
    {
        AchievementsManager.GetCategory(AchievementCategories.General)?.SetDefaultIcon(() => TouRoleIcons.Mayor.LoadAsset());
        AchievementsManager.GetCategory(AchievementCategories.Crewmate)?.SetDefaultIcon(() => TouRoleIcons.Crewmate.LoadAsset());
        AchievementsManager.GetCategory(AchievementCategories.Impostor)?.SetDefaultIcon(() => TouRoleIcons.Impostor.LoadAsset());
        AchievementsManager.GetCategory(AchievementCategories.Neutral)?.SetDefaultIcon(() => TouRoleIcons.Neutral.LoadAsset());
    }
}
