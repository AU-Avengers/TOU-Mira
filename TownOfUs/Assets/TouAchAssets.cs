using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAchAssets
{
    internal const string ShortPath = "TownOfUs.Resources.Achievements";

    public static LoadableAsset<Sprite> AchievementBox { get; } =
        new LoadableResourceAsset($"{ShortPath}.AchievementBox.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> AchievementToast { get; } =
        new LoadableResourceAsset($"{ShortPath}.AchievementToast.png", TownOfUsPlugin.TouAssembly);

    // Tried My Best
    public static LoadableAsset<Sprite> LuckyDice { get; } =
    new LoadableResourceAsset($"{ShortPath}.LuckyDice.png", TownOfUsPlugin.TouAssembly);

    // Crewmates
    public static LoadableAsset<Sprite> ForceBeWithYOu { get; } =
        new LoadableResourceAsset($"{ShortPath}.ForceBeWithYou.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> BloodyHands { get; } =
        new LoadableResourceAsset($"{ShortPath}.BloodyHands.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> RightOnTime { get; } =
        new LoadableResourceAsset($"{ShortPath}.RightOnTime.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> DeathByDemocracy { get; } =
        new LoadableResourceAsset($"{ShortPath}.DeathByDemocracy.png", TownOfUsPlugin.TouAssembly);

    // Impostors
    public static LoadableAsset<Sprite> FailedAmbush { get; } =
        new LoadableResourceAsset($"{ShortPath}.FailedAmbush.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> PublicExecution { get; } =
        new LoadableResourceAsset($"{ShortPath}.PublicExecution.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> Delusional { get; } =
        new LoadableResourceAsset($"{ShortPath}.Delusional.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> Spotless { get; } =
        new LoadableResourceAsset($"{ShortPath}.Spotless.png", TownOfUsPlugin.TouAssembly);

    // Neutrals
    public static LoadableAsset<Sprite> FairlyBadParent { get; } =
        new LoadableResourceAsset($"{ShortPath}.FairlyBadParent.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> HeatOfTheBattle { get; } =
        new LoadableResourceAsset($"{ShortPath}.HeatOfTheBattle.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> CommonCold { get; } =
        new LoadableResourceAsset($"{ShortPath}.CommonCold.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> AllBarkNoBite { get; } =
        new LoadableResourceAsset($"{ShortPath}.AllBarkNoBite.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> FullCourseMeal { get; } =
        new LoadableResourceAsset($"{ShortPath}.FullCourseMeal.png", TownOfUsPlugin.TouAssembly);

    // Other
    public static LoadableAsset<Sprite> PartyOfTwo { get; } =
        new LoadableResourceAsset($"{ShortPath}.PartyOfTwo.png", TownOfUsPlugin.TouAssembly);
    public static LoadableAsset<Sprite> ButItRefused { get; } =
        new LoadableResourceAsset($"{ShortPath}.ButItRefused.png", TownOfUsPlugin.TouAssembly);
}
