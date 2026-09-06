using UnityEngine;

namespace TownOfUs.Assets;

public static class TouNeutAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> RememberButtonSprite { get; } =
        new LoadableBundleSubAsset("RememberButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ProtectSprite { get; } =
        new LoadableBundleSubAsset("ProtectButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> GuardSprite { get; } =
        new LoadableBundleSubAsset("GuardButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BribeSprite { get; } =
        new LoadableBundleSubAsset("BribeButton", TouAssets.AbilityHolder);
    
    public static LoadableAsset<Sprite> ShiftSprite { get; } =
        new LoadableBundleSubAsset("ShiftButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> VestSprite { get; } =
        new LoadableBundleSubAsset("VestButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> Observe { get; } =
        new LoadableBundleSubAsset("ObserveButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ExeTormentSprite { get; } =
        new LoadableBundleSubAsset("ExeTormentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> JesterPokeSprite { get; } =
        new LoadableBundleSubAsset("JesterPokeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> JesterHauntSprite { get; } =
        new LoadableBundleSubAsset("JesterHauntButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> JesterVentSprite { get; } =
        new LoadableBundleSubAsset("JesterVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PhantomSpookSprite { get; } =
        new LoadableBundleSubAsset("PhantomSpookButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DouseButtonSprite { get; } =
        new LoadableBundleSubAsset("DouseButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> IgniteButtonSprite { get; } =
        new LoadableBundleSubAsset("IgniteButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ArsoVentSprite { get; } =
        new LoadableBundleSubAsset("ArsoVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HackSprite { get; } =
        new LoadableBundleSubAsset("HackButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MimicSprite { get; } =
        new LoadableBundleSubAsset("MimicButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> GlitchVentSprite { get; } =
        new LoadableBundleSubAsset("GlitchVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> GlitchKillSprite { get; } =
        new LoadableBundleSubAsset("GlitchKillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> JuggKillSprite { get; } =
        new LoadableBundleSubAsset("JuggKillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> JuggVentSprite { get; } =
        new LoadableBundleSubAsset("JuggVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PetrifySprite { get; } =
        new LoadableBundleSubAsset("MedusaPetrifyButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> StoneGazeSprite { get; } =
        new LoadableBundleSubAsset("MedusaStoneGazeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MedusaVentSprite { get; } =
        new LoadableBundleSubAsset("MedusaVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> InfectSprite { get; } =
        new LoadableBundleSubAsset("InfectButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PestKillSprite { get; } =
        new LoadableBundleSubAsset("PestKillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PestVentSprite { get; } =
        new LoadableBundleSubAsset("PestVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ReapSprite { get; } =
        new LoadableBundleSubAsset("ReapButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ReaperVentSprite { get; } =
        new LoadableBundleSubAsset("ReaperVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BiteSprite { get; } =
        new LoadableBundleSubAsset("BiteButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> VampVentSprite { get; } =
        new LoadableBundleSubAsset("VampVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> RampageSprite { get; } =
        new LoadableBundleSubAsset("RampageButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> WerewolfKillSprite { get; } =
        new LoadableBundleSubAsset("WolfKillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> WerewolfVentSprite { get; } =
        new LoadableBundleSubAsset("WolfVentButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> InquisKillSprite { get; } =
        new LoadableBundleSubAsset("InquisKillButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> InquireSprite { get; } =
        new LoadableBundleSubAsset("InquireButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ChefCookSprite { get; } =
        new LoadableBundleSubAsset("CookButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> ChefServeEmptySprite { get; } =
        new LoadableBundleSubAsset("ServeEmptyButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> ChefServeSalmonSprite { get; } =
        new LoadableBundleSubAsset("ServeSalmonButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> ChefServeCakeSprite { get; } =
        new LoadableBundleSubAsset("ServeCakeButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> ChefServeBurgerSprite { get; } =
        new LoadableBundleSubAsset("ServeBurgerButton", TouAssets.AbilityHolder);
    public static LoadableAsset<Sprite> ChefServeTurkeySprite { get; } =
        new LoadableBundleSubAsset("ServeTurkeyButton", TouAssets.AbilityHolder);

    public static List<LoadableAsset<Sprite>> ChefServeSprites { get; set; } =
    [
        ChefServeEmptySprite,
        ChefServeSalmonSprite,
        ChefServeCakeSprite,
        ChefServeBurgerSprite,
        ChefServeTurkeySprite
    ];

}