using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Assets;

public static class LegacyAssets
{
    public static bool IsLegacy =>
        TownOfUsPlugin.LegacyMode.Value is LegacyVisuals.Art or LegacyVisuals.Full;
    public static readonly AssetBundle MainBundle = AssetBundleManager.Load("legacy-assets");
    public static LoadableAsset<Sprite> Banner { get; } = new LoadableResourceAsset($"{TouAssets.ShortPath}.BannerLegacy.png", 34f);
    public static LoadableAsset<Sprite> BroadcastSprite { get; } =
        new LoadableBundleAsset<Sprite>("Detect", MainBundle);

    public static LoadableAsset<Sprite> DisperseSprite { get; } =
        new LoadableBundleAsset<Sprite>("Disperse", MainBundle);

    public static LoadableAsset<Sprite> BarryButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Button", MainBundle);

    public static LoadableAsset<Sprite> HysteriaSprite { get; } =
        new LoadableBundleAsset<Sprite>("Hysteria", MainBundle);

    public static LoadableAsset<Sprite> BlackmailLetterSprite { get; } =
        new LoadableBundleAsset<Sprite>("BlackmailLetter", MainBundle);

    public static LoadableAsset<Sprite> BlackmailOverlaySprite { get; } =
        new LoadableBundleAsset<Sprite>("BlackmailOverlay", MainBundle);

    public static LoadableAsset<Sprite> SwapActive { get; } =
        new LoadableBundleAsset<Sprite>("SwapperSwitch.png", MainBundle);

    public static LoadableAsset<Sprite> SwapInactive { get; } =
        new LoadableBundleAsset<Sprite>("SwapperSwitchDisabled.png", MainBundle);

    public static LoadableAsset<Sprite> RevealButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Reveal", MainBundle);

    public static LoadableAsset<Sprite> JailCellSprite { get; } =
        new LoadableBundleAsset<Sprite>("JailCell", MainBundle);

    public static LoadableAsset<Sprite> ImitateSelectSprite { get; } =
        new LoadableBundleAsset<Sprite>("ImitateSelect.png", MainBundle);

    public static LoadableAsset<Sprite> ImitateDeselectSprite { get; } =
        new LoadableBundleAsset<Sprite>("ImitateDeselect.png", MainBundle);

    public static LoadableAsset<Sprite> ShootMeetingSprite { get; } =
        new LoadableBundleAsset<Sprite>("Shoot.png", MainBundle);

    public static LoadableAsset<Sprite> ExecuteSprite { get; } =
        new LoadableBundleAsset<Sprite>("Execute.png", MainBundle);

    public static LoadableAsset<Sprite> Hacked { get; } = new LoadableBundleAsset<Sprite>("Lock", MainBundle);

    public static LoadableAsset<Sprite> BarricadeVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("Barricade.png", MainBundle);
}
