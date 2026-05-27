using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class LegacyVanillaAssets
{
    public static AssetBundle MainBundle => LegacyAssets.MainBundle;
    public static string LangSuffix => "-EnUS";

    public static LoadableAsset<Sprite> KillSprite { get; } = new LoadableBundleAsset<Sprite>($"KillButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> ReportSprite { get; } = new LoadableBundleAsset<Sprite>($"ReportButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> SabotageSprite { get; } = new LoadableBundleAsset<Sprite>($"SabotageButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> VentSprite { get; } = new LoadableBundleAsset<Sprite>($"VentButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> UseSprite { get; } = new LoadableBundleAsset<Sprite>($"UseButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> PetSprite { get; } = new LoadableBundleAsset<Sprite>($"PetButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> CustomizeSprite { get; } = new LoadableBundleAsset<Sprite>($"CustomizeButton{LangSuffix}", MainBundle);

    // Security Systems
    public static LoadableAsset<Sprite> DoorlogSprite { get; } = new LoadableBundleAsset<Sprite>($"DoorlogButton{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> VitalsSprite { get; } = new LoadableBundleAsset<Sprite>($"Vitals{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> SecuritySprite { get; } = new LoadableBundleAsset<Sprite>($"Security{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> AdminSkeldSprite { get; } = new LoadableBundleAsset<Sprite>($"AdminButtonSkeld{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> AdminMiraSprite { get; } = new LoadableBundleAsset<Sprite>($"AdminButtonMira{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> AdminPolusSprite { get; } = new LoadableBundleAsset<Sprite>($"AdminButtonPolus{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> AdminAirshipSprite { get; } = new LoadableBundleAsset<Sprite>($"AdminButtonAirship{LangSuffix}", MainBundle);

    // Other UI
    public static LoadableAsset<Sprite> StartSprite { get; } = new LoadableBundleAsset<Sprite>($"Start{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> PrivateSprite { get; } = new LoadableBundleAsset<Sprite>($"Private{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> PublicSprite { get; } = new LoadableBundleAsset<Sprite>($"Public{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> PlayAgainSprite { get; } = new LoadableBundleAsset<Sprite>($"PlayAgain{LangSuffix}", MainBundle);
    public static LoadableAsset<Sprite> QuitSprite { get; } = new LoadableBundleAsset<Sprite>($"Quit{LangSuffix}", MainBundle);
}
