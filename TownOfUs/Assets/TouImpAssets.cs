using UnityEngine;

namespace TownOfUs.Assets;

public static class TouImpAssets
{
    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> MarkSprite { get; } =
        new LoadableBundleSubAsset("MarkButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> RecallSprite { get; } =
        new LoadableBundleSubAsset("RecallButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> FlashSprite { get; } =
        new LoadableBundleSubAsset("FlashButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BlindSprite { get; } =
        new LoadableBundleSubAsset("BlindButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> SampleSprite { get; } =
        new LoadableBundleSubAsset("SampleButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MorphSprite { get; } =
        new LoadableBundleSubAsset("MorphButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> OvertakeSprite { get; } =
        new LoadableBundleSubAsset("OvertakeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> SwoopSprite { get; } =
        new LoadableBundleSubAsset("SwoopButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> UnswoopSprite { get; } =
        new LoadableBundleSubAsset("UnswoopButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> NoAbilitySprite { get; } =
        new LoadableBundleSubAsset("NoAbilityButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> CamouflageSprite { get; } =
        new LoadableBundleSubAsset("CamouflageButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> SprintSprite { get; } =
        new LoadableBundleSubAsset("CamoSprintButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> FreezeSprite { get; } =
        new LoadableBundleSubAsset("CamoSprintFreezeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PursueSprite { get; } =
        new LoadableBundleSubAsset("PursueButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> AmbushSprite { get; } =
        new LoadableBundleSubAsset("AmbushButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PlaceSprite { get; } =
        new LoadableBundleSubAsset("PlaceButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DetonatingSprite { get; } =
        new LoadableBundleSubAsset("DetonatingButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PlantSprite { get; } =
        new LoadableBundleSubAsset("PlantButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PoisonSprite { get; } =
        new LoadableBundleSubAsset("PoisonButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> PoisonedSprite { get; } =
        new LoadableBundleSubAsset("PoisonButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> ControlSprite { get; } =
        new LoadableBundleSubAsset("ControlButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HexSprite { get; } =
        new LoadableBundleSubAsset("HexButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HexBombSprite { get; } =
        new LoadableBundleSubAsset("HexBombButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> TraitorSelect { get; } =
        new LoadableBundleSubAsset("TraitorSelect", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> BlackmailSprite { get; } =
        new LoadableBundleSubAsset("BlackmailButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HypnotiseButtonSprite { get; } =
        new LoadableBundleSubAsset("HypnotiseButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> CleanButtonSprite { get; } =
        new LoadableBundleSubAsset("CleanButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> MineSprite { get; } =
        new LoadableBundleSubAsset("MineButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DragSprite { get; } =
        new LoadableBundleSubAsset("DragButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DropSprite { get; } =
        new LoadableBundleSubAsset("DropButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HerbConfuseSprite { get; } =
        new LoadableBundleSubAsset("HerbConfuseButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HerbExposeSprite { get; } =
        new LoadableBundleSubAsset("HerbExposeButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> HerbProtectSprite { get; } =
        new LoadableBundleSubAsset("HerbProtectButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DrinkRoleblockSprite { get; } =
        new LoadableBundleSubAsset("WineRoleblockButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DrinkSickenSprite { get; } =
        new LoadableBundleSubAsset("WineSickenButton", TouAssets.AbilityHolder);

    public static LoadableAsset<Sprite> DrinkPoisonSprite { get; } =
        new LoadableBundleSubAsset("WinePoisonButton", TouAssets.AbilityHolder);
}