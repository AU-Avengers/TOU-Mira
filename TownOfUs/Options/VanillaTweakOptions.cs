using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace TownOfUs.Options;

public sealed class VanillaTweakOptions : AbstractOptionGroup
{
    public override string GroupName => TouLocale.Get("TouOptionTitleVanillaTweaks");
    public override uint GroupPriority => 1;

    public ModdedToggleOption HideNamesOutOfSight { get; set; } =
        new("TouOptionHideNamesOutOfSight", false);

    public ModdedToggleOption TickCooldownsInMinigame { get; set; } =
        new("TouOptionTickCooldownsInMinigame", true);

    public ModdedToggleOption ParallelMedbay { get; set; } =
        new("TouOptionParallelMedbay", true);

    public ModdedToggleOption MedscanWalk { get; set; } =
        new("TouOptionMedscanWalk", true);

    public ModdedEnumOption SkipButtonDisable { get; set; } =
        new("TouOptionSkipButtonDisable", (int)SkipState.No,
            typeof(SkipState),
            [
                "TouOptionSkipButtonDisableEnumNever",
                "TouOptionSkipButtonDisableEnumEmergency",
                "TouOptionSkipButtonDisableEnumAlways"
            ]);

    public ModdedToggleOption HideVentAnimationNotInVision { get; set; } =
        new("TouOptionHideVentAnimationNotInVision", true);

    public ModdedEnumOption ShowPetsMode { get; set; } =
        new("TouOptionShowPetsMode", (int)PetVisiblity.AlwaysVisible,
            typeof(PetVisiblity),
            [
                "TouOptionShowPetsModeEnumClientSide",
                "TouOptionShowPetsModeEnumWhenAlive",
                "TouOptionShowPetsModeEnumAlwaysVisible"
            ]);

    public ModdedEnumOption HidePetsOnBodyRemove { get; set; } =
        new("TouOptionHidePetsOnBodyRemove", (int)PetHidden.DuringRound,
            typeof(PetHidden),
            [
                "TouOptionHidePetsOnBodyRemoveEnumNever",
                "TouOptionHidePetsOnBodyRemoveEnumDuringRound",
                "TouOptionHidePetsOnBodyRemoveEnumAlways"
            ])
        {
            Visible = () =>
                (PetVisiblity)OptionGroupSingleton<VanillaTweakOptions>.Instance.ShowPetsMode.Value
                is not PetVisiblity.WhenAlive
        };

    public bool CanPauseCooldown => !TickCooldownsInMinigame.Value &&
                                   (Minigame.Instance &&
                                    Minigame.Instance is not IngameWikiMinigame);

    public PetHidden PetVisibilityUponDeath =>
        ((PetVisiblity)ShowPetsMode.Value is PetVisiblity.WhenAlive)
            ? PetHidden.Never
            : (PetHidden)HidePetsOnBodyRemove.Value;
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