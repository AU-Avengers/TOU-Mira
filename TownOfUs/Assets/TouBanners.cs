using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouBanners
{
    // THIS FILE SHOULD ONLY ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs

    public static LoadableAsset<Sprite> PlaceholderRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("WipBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> AurialRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("AurialBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ForensicRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("ForensicBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> InvestigatorRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("InvestigatorBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> LookoutRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("LookoutBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MediumRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("MediumBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MysticRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("MysticBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SeerRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SeerBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SnitchRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SnitchBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SpyRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SpyBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SonarRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SonarBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> TrapperRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("TrapperBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> DeputyRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("DeputyBanner", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> HunterRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("HunterBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SheriffRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SheriffBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ProsecutorRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("ProsecutorBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ClericRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("ClericBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MedicRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("MedicBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> EngineerRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("EngineerBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SentryRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SentryBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> HaunterRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("HaunterBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> JesterRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("JesterBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SpectreRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("SpectreBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> EscapistRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("EscapistBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MinerRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("MinerBanner", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> UndertakerRoleBanner { get; } =
        new LoadableBundleAsset<Sprite>("UndertakerBanner", TouAssets.MainBundle);
}