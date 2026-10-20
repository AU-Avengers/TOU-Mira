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
    public static LoadableAsset<Sprite> ForceBeWithYOu { get; } =
        new LoadableResourceAsset($"{ShortPath}.ForceBeWithYou.png");
    public static LoadableAsset<Sprite> BloodyHands { get; } =
        new LoadableResourceAsset($"{ShortPath}.BloodyHands.png");
    public static LoadableAsset<Sprite> RightOnTime { get; } =
        new LoadableResourceAsset($"{ShortPath}.RightOnTime.png");
    public static LoadableAsset<Sprite> DeathByDemocracy { get; } =
        new LoadableResourceAsset($"{ShortPath}.DeathByDemocracy.png");

    // Impostors
    public static LoadableAsset<Sprite> FailedAmbush { get; } =
        new LoadableResourceAsset($"{ShortPath}.FailedAmbush.png");
    public static LoadableAsset<Sprite> PublicExecution { get; } =
        new LoadableResourceAsset($"{ShortPath}.PublicExecution.png");

    // Neutrals
    public static LoadableAsset<Sprite> FairlyBadParent { get; } =
        new LoadableResourceAsset($"{ShortPath}.FairlyBadParent.png");
    public static LoadableAsset<Sprite> HeatOfTheBattle { get; } =
        new LoadableResourceAsset($"{ShortPath}.HeatOfTheBattle.png");
    public static LoadableAsset<Sprite> CommonCold { get; } =
        new LoadableResourceAsset($"{ShortPath}.CommonCold.png");
    public static LoadableAsset<Sprite> AllBarkNoBite { get; } =
        new LoadableResourceAsset($"{ShortPath}.AllBarkNoBite.png");
    public static LoadableAsset<Sprite> FullCourseMeal { get; } =
        new LoadableResourceAsset($"{ShortPath}.FullCourseMeal.png");

    // Other
    public static LoadableAsset<Sprite> PartyOfTwo { get; } =
        new LoadableResourceAsset($"{ShortPath}.PartyOfTwo.png");
    public static LoadableAsset<Sprite> ButItRefused { get; } =
        new LoadableResourceAsset($"{ShortPath}.ButItRefused.png");
}
