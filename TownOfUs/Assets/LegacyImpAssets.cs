using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class LegacyImpAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN LegacyAssets.cs
    public static LoadableAsset<Sprite> MarkSprite { get; } =
        new LoadableBundleAsset<Sprite>("Mark", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> RecallSprite { get; } =
        new LoadableBundleAsset<Sprite>("Recall", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> FlashSprite { get; } =
        new LoadableBundleAsset<Sprite>("Flash", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BlindSprite { get; } =
        new LoadableBundleAsset<Sprite>("Blind", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> SampleSprite { get; } =
        new LoadableBundleAsset<Sprite>("Sample", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> MorphSprite { get; } =
        new LoadableBundleAsset<Sprite>("Morph", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> SwoopSprite { get; } =
        new LoadableBundleAsset<Sprite>("Swoop", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> NoAbilitySprite { get; } =
        new LoadableBundleAsset<Sprite>("NoAbility", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> CamouflageSprite { get; } =
        new LoadableBundleAsset<Sprite>("Camouflage", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> SprintSprite { get; } =
        new LoadableBundleAsset<Sprite>("CamoSprint", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> FreezeSprite { get; } =
        new LoadableBundleAsset<Sprite>("CamoSprintFreeze", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> PlantSprite { get; } =
        new LoadableBundleAsset<Sprite>("Plant", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> DetonatingSprite { get; } =
        new LoadableBundleAsset<Sprite>("Detonate", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BlackmailSprite { get; } =
        new LoadableBundleAsset<Sprite>("Blackmail", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> HypnotiseButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Hypnotise", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> CleanButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Janitor", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> MineSprite { get; } =
        new LoadableBundleAsset<Sprite>("Mine", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> DragSprite { get; } =
        new LoadableBundleAsset<Sprite>("Drag", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> DropSprite { get; } =
        new LoadableBundleAsset<Sprite>("Drop", LegacyAssets.MainBundle);
}