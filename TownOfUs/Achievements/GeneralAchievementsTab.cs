using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class GeneralAchievementsTab : AchievementsTab
{
    /// <inheritdoc/>
    public override string Name => "General Achievements";

    /// <inheritdoc/>
    public override Color GetTabColor() => TownOfUsColors.Politician;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Mayor.LoadAsset();
    public BaseBundleAchievement MiraMonday { get; set; } = new("Mira Monday",
        "Play on Mira HQ on a Monday!", TouAssets.IconMira, null!);
    public BaseBundleAchievement FungleFriday { get; set; } = new("Fungle Friday",
        "Play on Fungle on a Friday!", TouAssets.IconFungle, null!);
    public BaseBundleAchievement SubmergedSaturday { get; set; } = new("Submerged Saturday",
        "Play on Submerged on a Saturday!", TouAssets.IconSubmerged, null!);
    public BaseBundleAchievement SkeldSunday { get; set; } = new("Skeld Sunday",
        "Play on Skeld on a Sunday!", TouAssets.IconSkeld, null!);

}