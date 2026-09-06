using UnityEngine;

namespace TownOfUs.Assets;

public static class TouCrewAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs

    public static LoadableAsset<Sprite> CrewSwoopSprite { get; } =
        new LoadableBundleSubAsset("CrewSwoopButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> CrewUnswoopSprite { get; } =
        new LoadableBundleSubAsset("CrewUnswoopButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> InspectSprite { get; } =
        new LoadableBundleSubAsset("InspectButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> ExamineSprite { get; } =
        new LoadableBundleSubAsset("ExamineButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> WatchSprite { get; } =
        new LoadableBundleSubAsset("WatchButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ConfessSprite { get; } =
        new LoadableBundleSubAsset("ConfessButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BlessSprite { get; } =
        new LoadableBundleSubAsset("BlessButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> SeerSprite { get; } =
        new LoadableBundleSubAsset("SeerButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> GazeSprite { get; } =
        new LoadableBundleSubAsset("GazeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> IntuitSprite { get; } =
        new LoadableBundleSubAsset("IntuitButton", TouAssets.AbilityHolder);

    public static List<LoadableAsset<Sprite>> SeerButtonSprites { get; set; } =
    [
        SeerSprite,
        GazeSprite,
        IntuitSprite,
    ];

    public static LoadableAsset<Sprite> KnightSprite { get; } =
        new LoadableBundleSubAsset("KnightButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> TrackSprite { get; } =
        new LoadableBundleSubAsset("TrackButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> TrapSprite { get; } =
        new LoadableBundleSubAsset("TrapButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> CampButtonSprite { get; } =
        new LoadableBundleSubAsset("CampButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> StalkButtonSprite { get; } =
        new LoadableBundleSubAsset("StalkButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> JailSprite { get; } =
        new LoadableBundleSubAsset("JailButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> AlertSprite { get; } =
        new LoadableBundleSubAsset("AlertButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HunterKillSprite { get; } =
        new LoadableBundleSubAsset("HunterKillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> OfficerShootSprite { get; } =
        new LoadableBundleSubAsset("OfficerShootButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> OfficerLoadSprite { get; } =
        new LoadableBundleSubAsset("OfficerLoadButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> SheriffShootSprite { get; } =
        new LoadableBundleSubAsset("SheriffShootButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ReviveSprite { get; } =
        new LoadableBundleSubAsset("ReviveButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> CleanseSprite { get; } =
        new LoadableBundleSubAsset("CleanseButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BarrierSprite { get; } =
        new LoadableBundleSubAsset("BarrierButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MedicSprite { get; } =
        new LoadableBundleSubAsset("MedicButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MagicMirrorSprite { get; } =
        new LoadableBundleSubAsset("MagicMirrorButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> UnleashSprite { get; } =
        new LoadableBundleSubAsset("UnleashButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> FortifySprite { get; } =
        new LoadableBundleSubAsset("FortifyButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> FixButtonSprite { get; } =
        new LoadableBundleSubAsset("FixButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> EngiVentSprite { get; } =
        new LoadableBundleSubAsset("EngiVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MediateSprite { get; } =
        new LoadableBundleSubAsset("MediateButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> CampaignButtonSprite { get; } =
        new LoadableBundleSubAsset("CampaignButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> RoleblockSprite { get; } =
        new LoadableBundleSubAsset("BeerRoleblockButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> SpillSprite { get; } =
        new LoadableBundleSubAsset("BeerSpillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> FlushSprite { get; } =
        new LoadableBundleSubAsset("FlushButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BlockSprite { get; } =
        new LoadableBundleSubAsset("BarricadeButton", TouAssets.AbilityHolder);
    
    public static LoadableAsset<Sprite> RewindSprite { get; } =
        new LoadableBundleSubAsset("RewindButton", TouAssets.AbilityHolder);
    
    public static LoadableAsset<Sprite> RewindingSprite { get; } =
        new LoadableBundleSubAsset("RewindingButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> Transport { get; } =
        new LoadableBundleSubAsset("TransportButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> OverchargeSprite { get; } =
        new LoadableBundleSubAsset("OverchargeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DeployCamSprite { get; } =
        new LoadableBundleSubAsset("DeployButton", TouAssets.AbilityHolder);
}