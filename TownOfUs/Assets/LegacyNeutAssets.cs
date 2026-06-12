using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class LegacyNeutAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN LegacyAssets.cs
    public static LoadableAsset<Sprite> RememberButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Remember", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> ProtectSprite { get; } =
        new LoadableBundleAsset<Sprite>("Protect", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> GuardSprite { get; } =
        new LoadableBundleAsset<Sprite>("Guard", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BribeSprite { get; } =
        new LoadableBundleAsset<Sprite>("Bribe", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> VestSprite { get; } =
        new LoadableBundleAsset<Sprite>("Vest", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> Observe { get; } =
        new LoadableBundleAsset<Sprite>("Observe", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> DouseButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Douse", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> IgniteButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Ignite", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> HackSprite { get; } =
        new LoadableBundleAsset<Sprite>("Hack", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> MimicSprite { get; } =
        new LoadableBundleAsset<Sprite>("Mimic", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> InfectSprite { get; } =
        new LoadableBundleAsset<Sprite>("Infect", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> ReapSprite { get; } =
        new LoadableBundleAsset<Sprite>("Reap", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> BiteSprite { get; } =
        new LoadableBundleAsset<Sprite>("Bite", LegacyAssets.MainBundle);

    public static LoadableAsset<Sprite> RampageSprite { get; } =
        new LoadableBundleAsset<Sprite>("Rampage", LegacyAssets.MainBundle);

}