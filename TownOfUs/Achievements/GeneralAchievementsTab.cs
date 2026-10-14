using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class GeneralAchievementsTab : TouAchievementsTab
{
    /// <inheritdoc/>
    public override string Name => "General Achievements";

    /// <inheritdoc/>
    public override string CategoryId => AchievementCategories.General;

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

    public BaseBundleAchievement Cheater { get; set; } = new("Cheater",
        "Use /up to give yourself a role in the lobby.", TouRoleIcons.Impostor, null!, 2, true);

    public BaseBundleAchievement Silence { get; set; } = new("Silence",
        "Force end a meeting as host.", TouRoleIcons.Prosecutor, null!, 2, true);

    public BaseBundleAchievement Oops { get; set; } = new("Oops",
        "Abort a game as host.", TouRoleIcons.Mayor, null!, 2, true);

    public BaseBundleAchievement IveSeenThePlays { get; set; } = new("I've seen the plays",
        "No I'm actually inclined to believe 5up you know because I've seen the plays where dk hops out of a vent and then kills himself",
        TouRoleIcons.Jester, null!, 2, true);

}