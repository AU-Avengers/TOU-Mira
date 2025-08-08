using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouCrewAssets
{
    private const string ShortPath = "TownOfUs.Resources";
    private const string ButtonPath = $"{ShortPath}.CrewButtons";
    private const string BannerPath = $"{ShortPath}.RoleBanners";

    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> InspectSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.InspectButton.png");

    public static LoadableAsset<Sprite> ExamineSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ExamineButton.png");

    public static LoadableAsset<Sprite> WatchSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.WatchButton.png");

    public static LoadableAsset<Sprite> ConfessSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ConfessButton.png");

    public static LoadableAsset<Sprite> BlessSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.BlessButton.png");

    public static LoadableAsset<Sprite> SeerSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.SeerButton.png");
    public static LoadableAsset<Sprite> PsychicSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.PsychicButton.png");
    public static LoadableAsset<Sprite> ProphecySprite { get; } = new LoadableResourceAsset($"{ButtonPath}.ProphecyButton.png");

    public static LoadableAsset<Sprite> TrackSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.TrackButton.png");

    public static LoadableAsset<Sprite> TrapSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.TrapButton.png");

    public static LoadableAsset<Sprite> CampButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.CampButton.png");

    public static LoadableAsset<Sprite> StalkButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.StalkButton.png");

    public static LoadableAsset<Sprite> JailSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.JailButton.png");

    public static LoadableAsset<Sprite> AlertSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.AlertButton.png");

    public static LoadableAsset<Sprite> HunterKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.HunterKillButton.png");

    public static LoadableAsset<Sprite> SheriffShootSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.SheriffShootButton.png");

    public static LoadableAsset<Sprite> ReviveSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ReviveButton.png");

    public static LoadableAsset<Sprite> CleanseSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.CleanseButton.png");

    public static LoadableAsset<Sprite> BarrierSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.BarrierButton.png");

    public static LoadableAsset<Sprite> MedicSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.MedicButton.png");

    public static LoadableAsset<Sprite> MagicMirrorSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.MagicMirrorButton.png");
    
    public static LoadableAsset<Sprite> UnleashSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.UnleashButton.png");
    public static LoadableAsset<Sprite> FortifySprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.FortifyButton.png");

    public static LoadableAsset<Sprite> FixButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.FixButton.png");

    public static LoadableAsset<Sprite> EngiVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.EngiVentButton.png");

    public static LoadableAsset<Sprite> MediateSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.MediateButton.png");

    public static LoadableAsset<Sprite> CampaignButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.CampaignButton.png");

    public static LoadableAsset<Sprite> FlushSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.FlushButton.png");

    public static LoadableAsset<Sprite> BarricadeSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.BarricadeButton.png");

    public static LoadableAsset<Sprite> Transport { get; } =
        new LoadableResourceAsset($"{ButtonPath}.TransportButton.png");

    public static LoadableAsset<Sprite> KnightButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.KnightButton.png");

    public static LoadableAsset<Sprite> RoleblockButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.RoleblockButton.png");

    public static LoadableAsset<Sprite> EngineerRoleBanner { get; } =
        new LoadableResourceAsset($"{BannerPath}.Engineer.png");
}