using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAchAssets
{
    internal const string ShortPath = "TownOfUs.Resources.Achievements";

    public static LoadableAsset<Sprite> AchievementBox { get; } =
        new LoadableResourceAsset($"{ShortPath}.AchievementBox.png");
    public static LoadableAsset<Sprite> AchievementToast { get; } =
        new LoadableResourceAsset($"{ShortPath}.AchievementToast.png");

    // Crewmates
    public static LoadableAsset<Sprite> BloodyHands { get; } =
        new LoadableResourceAsset($"{ShortPath}.BloodyHands.png");
    public static LoadableAsset<Sprite> RightOnTime { get; } =
        new LoadableResourceAsset($"{ShortPath}.RightOnTime.png");
    public static LoadableAsset<Sprite> DeathByDemocracy { get; } =
        new LoadableResourceAsset($"{ShortPath}.DeathByDemocracy.png");

    // Neutrals
    public static LoadableAsset<Sprite> FullCourseMeal { get; } =
        new LoadableResourceAsset($"{ShortPath}.FullCourseMeal.png");
}
