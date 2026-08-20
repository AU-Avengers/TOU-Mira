using UnityEngine;

namespace TownOfUs.Assets;

public static class TouBanners
{
    // THIS FILE SHOULD ONLY ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs

    public static LoadableAsset<Sprite> PlaceholderRoleBanner { get; } =
        new LoadableBundleSubAsset("WipBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> CrewmateRoleBanner { get; } =
        new LoadableBundleSubAsset("CrewmateBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> NeutralRoleBanner { get; } =
        new LoadableBundleSubAsset("NeutralBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> ImpostorRoleBanner { get; } =
        new LoadableBundleSubAsset("ImpostorBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> AurialRoleBanner { get; } =
        new LoadableBundleSubAsset("AurialBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> ForensicRoleBanner { get; } =
        new LoadableBundleSubAsset("ForensicBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> InvestigatorRoleBanner { get; } =
        new LoadableBundleSubAsset("InvestigatorBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> LookoutRoleBanner { get; } =
        new LoadableBundleSubAsset("LookoutBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> MediumRoleBanner { get; } =
        new LoadableBundleSubAsset("MediumBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> MysticRoleBanner { get; } =
        new LoadableBundleSubAsset("MysticBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SeerRoleBanner { get; } =
        new LoadableBundleSubAsset("SeerBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SnitchRoleBanner { get; } =
        new LoadableBundleSubAsset("SnitchBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SpyRoleBanner { get; } =
        new LoadableBundleSubAsset("SpyBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SonarRoleBanner { get; } =
        new LoadableBundleSubAsset("SonarBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> TrapperRoleBanner { get; } =
        new LoadableBundleSubAsset("TrapperBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> DeputyRoleBanner { get; } =
        new LoadableBundleSubAsset("DeputyBanner", TouAssets.RoleBannerHolder);
    public static LoadableAsset<Sprite> HunterRoleBanner { get; } =
        new LoadableBundleSubAsset("HunterBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SheriffRoleBanner { get; } =
        new LoadableBundleSubAsset("SheriffBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> ProsecutorRoleBanner { get; } =
        new LoadableBundleSubAsset("ProsecutorBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> ClericRoleBanner { get; } =
        new LoadableBundleSubAsset("ClericBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> MedicRoleBanner { get; } =
        new LoadableBundleSubAsset("MedicBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> EngineerRoleBanner { get; } =
        new LoadableBundleSubAsset("EngineerBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SentryRoleBanner { get; } =
        new LoadableBundleSubAsset("SentryBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> HaunterRoleBanner { get; } =
        new LoadableBundleSubAsset("HaunterBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> JesterRoleBanner { get; } =
        new LoadableBundleSubAsset("JesterBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> SpectreRoleBanner { get; } =
        new LoadableBundleSubAsset("SpectreBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> EscapistRoleBanner { get; } =
        new LoadableBundleSubAsset("EscapistBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> MinerRoleBanner { get; } =
        new LoadableBundleSubAsset("MinerBanner", TouAssets.RoleBannerHolder);

    public static LoadableAsset<Sprite> UndertakerRoleBanner { get; } =
        new LoadableBundleSubAsset("UndertakerBanner", TouAssets.RoleBannerHolder);
}