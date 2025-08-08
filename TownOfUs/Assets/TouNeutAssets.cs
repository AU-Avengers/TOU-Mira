using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouNeutAssets
{
    private const string ShortPath = "TownOfUs.Resources";

    // private const string BannerPath = $"{ShortPath}.RoleBanners"; // Commenting until it is used, so that the warnings stop screaming
    private const string ButtonPath = $"{ShortPath}.NeutButtons";

    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> RememberButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.RememberButton.png");

    public static LoadableAsset<Sprite> ProtectSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ProtectButton.png");

    public static LoadableAsset<Sprite> GuardSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.GuardButton.png");

    public static LoadableAsset<Sprite> BribeSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.BribeButton.png");

    public static LoadableAsset<Sprite> VestSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.VestButton.png");

    public static LoadableAsset<Sprite> Observe { get; } = new LoadableResourceAsset($"{ButtonPath}.ObserveButton.png");

    public static LoadableAsset<Sprite> ExeTormentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ExeTormentButton.png");
    public static LoadableAsset<Sprite> JesterHauntSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.JesterHauntButton.png");
    public static LoadableAsset<Sprite> JesterVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.JesterVentButton.png");

    public static LoadableAsset<Sprite> InquisKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.InquisKillButton.png");

    public static LoadableAsset<Sprite> InquireSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.InquireButton.png");
    
    public static LoadableAsset<Sprite> PhantomSpookSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.PhantomSpookButton.png");

    public static LoadableAsset<Sprite> DouseButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.DouseButton.png");

    public static LoadableAsset<Sprite> IgniteButtonSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.IgniteButton.png");

    public static LoadableAsset<Sprite> ArsoVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ArsoVentButton.png");

    public static LoadableAsset<Sprite> HackSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.HackButton.png");

    public static LoadableAsset<Sprite> MimicSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.MimicButton.png");

    public static LoadableAsset<Sprite> GlitchVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.GlitchVentButton.png");

    public static LoadableAsset<Sprite> GlitchKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.GlitchKillButton.png");

    public static LoadableAsset<Sprite> JuggKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.JuggKillButton.png");

    public static LoadableAsset<Sprite> JuggVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.JuggVentButton.png");

    public static LoadableAsset<Sprite> SerialKillerKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.SerialKillerKillButton.png");

    public static LoadableAsset<Sprite> SerialKillerVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.SerialKillerVentButton.png");

    public static LoadableAsset<Sprite> ShroudKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ShroudKillButton.png");

    public static LoadableAsset<Sprite> EnshroudSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.EnshroudButton.png");

    public static LoadableAsset<Sprite> CompelSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.CompelButton.png");

    public static LoadableAsset<Sprite> ShroudVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ShroudVentButton.png");

    public static LoadableAsset<Sprite> InfectSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.InfectButton.png");

    public static LoadableAsset<Sprite> PestKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.PestKillButton.png");

    public static LoadableAsset<Sprite> PestVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.PestVentButton.png");

    public static LoadableAsset<Sprite> ReapSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.ReapButton.png");

    public static LoadableAsset<Sprite> ReaperVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ReaperVentButton.png");

    public static LoadableAsset<Sprite> BiteSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.BiteButton.png");

    public static LoadableAsset<Sprite> VampVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.VampVentButton.png");

    public static LoadableAsset<Sprite> RampageSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.RampageButton.png");

    public static LoadableAsset<Sprite> WerewolfKillSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.WolfKillButton.png");

    public static LoadableAsset<Sprite> WerewolfVentSprite { get; } =
        new LoadableResourceAsset($"{ButtonPath}.WolfVentButton.png");
}