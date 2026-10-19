using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAchAssets
{
    internal const string ShortPath = "TownOfUs.Resources.Achievements";

    public static LoadableAsset<Sprite> AchievementBox { get; } =
        new LoadableResourceAsset($"{ShortPath}.AchievementBox.png");
    public static LoadableAsset<Sprite> AchievementToast { get; } =
        new LoadableResourceAsset($"{ShortPath}.AchievementToast.png");

    // Tried My Best
    public static LoadableAsset<Sprite> LuckyDice { get; } =
    new LoadableResourceAsset($"{ShortPath}.LuckyDice.png");

    // Crewmates
    public static LoadableAsset<Sprite> BloodyHands { get; } =
        new LoadableResourceAsset($"{ShortPath}.BloodyHands.png");
    public static LoadableAsset<Sprite> RightOnTime { get; } =
        new LoadableResourceAsset($"{ShortPath}.RightOnTime.png");
    public static LoadableAsset<Sprite> DeathByDemocracy { get; } =
        new LoadableResourceAsset($"{ShortPath}.DeathByDemocracy.png");

    // Neutrals
    public static LoadableAsset<Sprite> FairlyBadParent { get; } =
        new LoadableResourceAsset($"{ShortPath}.FairlyBadParent.png");
    public static LoadableAsset<Sprite> AllBarkNoBite { get; } =
        new LoadableResourceAsset($"{ShortPath}.AllBarkNoBite.png");
    public static LoadableAsset<Sprite> FullCourseMeal { get; } =
        new LoadableResourceAsset($"{ShortPath}.FullCourseMeal.png");
}
