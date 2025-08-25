using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouCrewAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> InspectSprite { get; } =
        new LoadableBundleAsset<Sprite>("InspectButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ExamineSprite { get; } =
        new LoadableBundleAsset<Sprite>("ExamineButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> WatchSprite { get; } =
        new LoadableBundleAsset<Sprite>("WatchButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ConfessSprite { get; } =
        new LoadableBundleAsset<Sprite>("ConfessButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BlessSprite { get; } =
        new LoadableBundleAsset<Sprite>("BlessButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SeerSprite { get; } = new LoadableBundleAsset<Sprite>("SeerButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> TrackSprite { get; } =
        new LoadableBundleAsset<Sprite>("TrackButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> TrapSprite { get; } = new LoadableBundleAsset<Sprite>("TrapButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> CampButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("CampButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> StalkButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("StalkButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> JailSprite { get; } = new LoadableBundleAsset<Sprite>("JailButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> AlertSprite { get; } =
        new LoadableBundleAsset<Sprite>("AlertButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> HunterKillSprite { get; } =
        new LoadableBundleAsset<Sprite>("HunterKillButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SheriffShootSprite { get; } =
        new LoadableBundleAsset<Sprite>("SheriffShootButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ReviveSprite { get; } =
        new LoadableBundleAsset<Sprite>("ReviveButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> CleanseSprite { get; } =
        new LoadableBundleAsset<Sprite>("CleanseButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BarrierSprite { get; } =
        new LoadableBundleAsset<Sprite>("BarrierButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MedicSprite { get; } =
        new LoadableBundleAsset<Sprite>("MedicButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MagicMirrorSprite { get; } =
        new LoadableBundleAsset<Sprite>("MagicMirrorButton", TouAssets.MainBundle);
    
    public static LoadableAsset<Sprite> UnleashSprite { get; } =
        new LoadableBundleAsset<Sprite>("UnleashButton", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> FortifySprite { get; } =
        new LoadableBundleAsset<Sprite>("FortifyButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> FixButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("FixButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> EngiVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("EngiVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MediateSprite { get; } =
        new LoadableBundleAsset<Sprite>("MediateButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> CampaignButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("CampaignButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> FlushSprite { get; } =
        new LoadableBundleAsset<Sprite>("FlushButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BarricadeSprite { get; } =
        new LoadableBundleAsset<Sprite>("BarricadeButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Transport { get; } =
        new LoadableBundleAsset<Sprite>("TransportButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> EngineerRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("EngineerBanner", TouAssets.MainBundle);
}