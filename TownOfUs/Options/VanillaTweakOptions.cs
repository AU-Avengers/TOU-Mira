using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class VanillaTweakOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.VanillaTweaks");
    public override uint GroupPriority => 1;
    public override LoadableAsset<Sprite> Sprite => TouRoleIcons.Scientist;

    public ModdedEnumOption<NameSight> HideNamesOutOfSight { get; set; } =
        new("TouOptionHideNamesOutOfSight", NameSight.Basic,
            [
                "TouOptionHideNamesOutOfSightEnumBasic",
                "TouOptionHideNamesOutOfSightEnumRoomBased",
                "TouOptionHideNamesOutOfSightEnumHarsh"
            ]);

    public ModdedToggleOption TickCooldownsInMinigame { get; set; } =
        new("TouOptionTickCooldownsInMinigame", true);

    public ModdedToggleOption ParallelMedbay { get; set; } =
        new("TouOptionParallelMedbay", true);

    public ModdedToggleOption MedscanWalk { get; set; } =
        new("TouOptionMedscanWalk", true);

    public ModdedEnumOption<SkipState> SkipButtonDisable { get; set; } =
        new("TouOptionSkipButtonDisable", SkipState.No,
            [
                "TouOptionSkipButtonDisableEnumNever",
                "TouOptionSkipButtonDisableEnumEmergency",
                "TouOptionSkipButtonDisableEnumAlways"
            ]);

    public ModdedToggleOption HideVentAnimationNotInVision { get; set; } =
        new("TouOptionHideVentAnimationNotInVision", true);

    public ModdedEnumOption<PetVisiblity> ShowPetsMode { get; set; } =
        new("TouOptionShowPetsMode", PetVisiblity.AlwaysVisible,
            [
                "TouOptionShowPetsModeEnumClientSide",
                "TouOptionShowPetsModeEnumWhenAlive",
                "TouOptionShowPetsModeEnumAlwaysVisible"
            ]);

    public ModdedEnumOption<PetHidden> HidePetsOnBodyRemove { get; set; } =
        new("TouOptionHidePetsOnBodyRemove", PetHidden.DuringRound,
            [
                "TouOptionHidePetsOnBodyRemoveEnumNever",
                "TouOptionHidePetsOnBodyRemoveEnumDuringRound",
                "TouOptionHidePetsOnBodyRemoveEnumAlways"
            ])
        {
            Visible = () =>
                OptionGroupSingleton<VanillaTweakOptions>.Instance.ShowPetsMode.Value
                is not PetVisiblity.WhenAlive
        };

    public bool CanPauseCooldown => !TickCooldownsInMinigame.Value &&
                                   (Minigame.Instance &&
                                    Minigame.Instance is not IngameWikiMinigame);

    public PetHidden PetVisibilityUponDeath =>
        (ShowPetsMode.Value is PetVisiblity.WhenAlive)
            ? PetHidden.Never
            : HidePetsOnBodyRemove.Value;
}
public enum NameSight
{
    Basic,
    RoomBased,
    Harsh
}

public enum SkipState
{
    No,
    Emergency,
    Always
}

public enum PetVisiblity
{
    ClientSide,
    WhenAlive,
    AlwaysVisible
}

public enum PetHidden
{
    Never,
    DuringRound,
    Remove
}