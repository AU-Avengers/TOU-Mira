using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouNeutAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> RememberButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("RememberButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ProtectSprite { get; } =
        new LoadableBundleAsset<Sprite>("ProtectButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> GuardSprite { get; } =
        new LoadableBundleAsset<Sprite>("GuardButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BribeSprite { get; } =
        new LoadableBundleAsset<Sprite>("BribeButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> VestSprite { get; } = new LoadableBundleAsset<Sprite>("VestButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Observe { get; } = new LoadableBundleAsset<Sprite>("ObserveButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ExeTormentSprite { get; } =
        new LoadableBundleAsset<Sprite>("ExeTormentButton", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> JesterHauntSprite { get; } =
        new LoadableBundleAsset<Sprite>("JesterHauntButton", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> JesterVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("JesterVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> InquisKillSprite { get; } =
        new LoadableBundleAsset<Sprite>("InquisKillButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> InquireSprite { get; } =
        new LoadableBundleAsset<Sprite>("InquireButton", TouAssets.MainBundle);
    
    public static LoadableAsset<Sprite> PhantomSpookSprite { get; } =
        new LoadableBundleAsset<Sprite>("PhantomSpookButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> DouseButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("DouseButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> IgniteButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("IgniteButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ArsoVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("ArsoVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> HackSprite { get; } = new LoadableBundleAsset<Sprite>("HackButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> MimicSprite { get; } =
        new LoadableBundleAsset<Sprite>("MimicButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> GlitchVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("GlitchVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> GlitchKillSprite { get; } =
        new LoadableBundleAsset<Sprite>("GlitchKillButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> JuggKillSprite { get; } =
        new LoadableBundleAsset<Sprite>("JuggKillButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> JuggVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("JuggVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> InfectSprite { get; } =
        new LoadableBundleAsset<Sprite>("InfectButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> PestKillSprite { get; } =
        new LoadableBundleAsset<Sprite>("PestKillButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> PestVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("PestVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ReapSprite { get; } = new LoadableBundleAsset<Sprite>("ReapButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> ReaperVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("ReaperVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> BiteSprite { get; } = new LoadableBundleAsset<Sprite>("BiteButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> VampVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("VampVentButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> RampageSprite { get; } =
        new LoadableBundleAsset<Sprite>("RampageButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> WerewolfKillSprite { get; } =
        new LoadableBundleAsset<Sprite>("WolfKillButton", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> WerewolfVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("WolfVentButton", TouAssets.MainBundle);
}