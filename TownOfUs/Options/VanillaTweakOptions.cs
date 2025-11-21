using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class VanillaTweakOptions : AbstractOptionGroup
{
    public override string GroupName => "Vanilla Tweaks";
    public override uint GroupPriority => 1;

    [ModdedToggleOption("Vanilla Roles are Guessable")]
    public bool GuessVanillaRoles { get; set; } = true;

    /*[ModdedToggleOption("Hide Names Out Of Sight")]
    public bool HideNamesOutOfSight { get; set; } = true;*/

    [ModdedNumberOption("Max Players Alive When Vents Disable", 1f, 15f, 1f, MiraNumberSuffixes.None, "0.#")]
    public float PlayerCountWhenVentsDisable { get; set; } = 2f;

    [ModdedToggleOption("Parallel Medbay Scans")]
    public bool ParallelMedbay { get; set; } = true;

    [ModdedEnumOption("Disable Meeting Skip Button", typeof(SkipState))]
    public SkipState SkipButtonDisable { get; set; } = SkipState.No;

    [ModdedToggleOption("Hide Vent Animations Not In Vision")]
    public bool HideVentAnimationNotInVision { get; set; } = true;
}
