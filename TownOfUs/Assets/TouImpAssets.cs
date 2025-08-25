using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouImpAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> MarkSprite { get; } = new LoadableBundleAsset<Sprite>("MarkButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> RecallSprite { get; } =
        new LoadableBundleAsset<Sprite>("RecallButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> FlashSprite { get; } =
        new LoadableBundleAsset<Sprite>("FlashButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BlindSprite { get; } =
        new LoadableBundleAsset<Sprite>("BlindButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SampleSprite { get; } =
        new LoadableBundleAsset<Sprite>("SampleButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MorphSprite { get; } =
        new LoadableBundleAsset<Sprite>("MorphButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SwoopSprite { get; } =
        new LoadableBundleAsset<Sprite>("SwoopButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> UnswoopSprite { get; } =
        new LoadableBundleAsset<Sprite>("UnswoopButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> NoAbilitySprite { get; } =
        new LoadableBundleAsset<Sprite>("NoAbilityButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> CamouflageSprite { get; } =
        new LoadableBundleAsset<Sprite>("CamouflageButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SprintSprite { get; } =
        new LoadableBundleAsset<Sprite>("CamoSprintButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> FreezeSprite { get; } =
        new LoadableBundleAsset<Sprite>("CamoSprintFreezeButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> PursueSprite { get; } =
        new LoadableBundleAsset<Sprite>("PursueButton", TouAssets.MainBundle);
    
    public static LoadableAsset<Sprite> AmbushSprite { get; } =
        new LoadableBundleAsset<Sprite>("AmbushButton", TouAssets.MainBundle);
    
    public static LoadableAsset<Sprite> PlaceSprite { get; } =
        new LoadableBundleAsset<Sprite>("PlaceButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> DetonatingSprite { get; } =
        new LoadableBundleAsset<Sprite>("DetonatingButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> PlantSprite { get; } =
        new LoadableBundleAsset<Sprite>("PlantButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> TraitorSelect { get; } =
        new LoadableBundleAsset<Sprite>("TraitorSelect", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BlackmailSprite { get; } =
        new LoadableBundleAsset<Sprite>("BlackmailButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> HypnotiseButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("HypnotiseButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> CleanButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("CleanButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MineSprite { get; } = new LoadableBundleAsset<Sprite>("MineButton", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> DragSprite { get; } = new LoadableBundleAsset<Sprite>("DragButton", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> DropSprite { get; } = new LoadableBundleAsset<Sprite>("DropButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MinerRoleBanner { get; } = new LoadableBundleAsset<Sprite>("MinerBanner", TouAssets.MainBundle);
}