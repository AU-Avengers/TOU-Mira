using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class LegacyCrewAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN LegacyAssets.cs

    public static LoadableAsset<Sprite> InspectSprite { get; } =
        new LoadableBundleAsset<Sprite>("Inspect", LegacyAssets.MainBundle);
    public static LoadableAsset<Sprite> ExamineSprite { get; } =
        new LoadableBundleAsset<Sprite>("Examine", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> WatchSprite { get; } =
        new LoadableBundleAsset<Sprite>("Watch", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> ConfessSprite { get; } =
        new LoadableBundleAsset<Sprite>("Confess", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BlessSprite { get; } =
        new LoadableBundleAsset<Sprite>("Bless", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> SeerSprite { get; } =
        new LoadableBundleAsset<Sprite>("Seer", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> TrackSprite { get; } =
        new LoadableBundleAsset<Sprite>("Track", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> TrapSprite { get; } =
        new LoadableBundleAsset<Sprite>("Trap", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> CampButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Camp", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> StalkButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Stalk", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> JailSprite { get; } =
        new LoadableBundleAsset<Sprite>("Jail", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> AlertSprite { get; } =
        new LoadableBundleAsset<Sprite>("Alert", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> ReviveSprite { get; } =
        new LoadableBundleAsset<Sprite>("Revive", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> CleanseSprite { get; } =
        new LoadableBundleAsset<Sprite>("Cleanse", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BarrierSprite { get; } =
        new LoadableBundleAsset<Sprite>("Barrier", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> MedicSprite { get; } =
        new LoadableBundleAsset<Sprite>("Medic", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> FortifySprite { get; } =
        new LoadableBundleAsset<Sprite>("Fortify", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> FixButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Engineer", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> MediateSprite { get; } =
        new LoadableBundleAsset<Sprite>("Mediate", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> CampaignButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Campaign", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> FlushSprite { get; } =
        new LoadableBundleAsset<Sprite>("Flush", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BlockSprite { get; } =
        new LoadableBundleAsset<Sprite>("Block", LegacyAssets.MainBundle);
    
    public static LoadableAsset<Sprite> RewindSprite { get; } =
        new LoadableBundleAsset<Sprite>("Rewind", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> Transport { get; } =
        new LoadableBundleAsset<Sprite>("Transport", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> DeployCamSprite { get; } =
        new LoadableBundleAsset<Sprite>("CreateCam", LegacyAssets.MainBundle);
}