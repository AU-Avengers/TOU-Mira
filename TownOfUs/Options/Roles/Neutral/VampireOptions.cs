using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class VampireOptions : AbstractOptionGroup<VampireRole>
{
    public override string GroupName => TouLocale.Get("TouRoleVampire", "Vampire");

    [ModdedNumberOption("TouOptionVampireBiteCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BiteCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionVampireMaxVamps", 2, 5, 1, MiraNumberSuffixes.None, "0")]
    public float MaxVampires { get; set; } = 2;

    [ModdedToggleOption("TouOptionVampireImpostorVision")]
    public bool HasVision { get; set; } = true;

    [ModdedToggleOption("TouOptionVampireNewVampsAssassinate")]
    public bool CanGuessAsNewVamp { get; set; } = true;

    public ModdedEnumOption<ValidBites> ValidConversions { get; } = new("TouOptionVampireValidNeutralConversions",
        ValidBites.BenignAndEvil,
        [
            "TouOptionVampireNeutConvertEnumOnlyCrew", "TouOptionVampireNeutConvertEnumOnlyBenign",
            "TouOptionVampireNeutConvertEnumOnlyEvil", "TouOptionVampireNeutConvertEnumOnlyOutlier",
            "TouOptionVampireNeutConvertEnumBenignEvil", "TouOptionVampireNeutConvertEnumBenignOutlier",
            "TouOptionVampireNeutConvertEnumEvilOutlier", "TouOptionVampireNeutConvertEnumMostNeuts"
        ]);

    public ModdedToggleOption ConvertLovers { get; set; } = new("TouOptionVampireConvertLovers", false);

    [ModdedToggleOption("TouOptionVampireNewVampiresConvert")]
    public bool CanConvertAsNewVamp { get; set; } = true;

    [ModdedToggleOption("TouOptionVampireCanVent")]
    public bool CanVent { get; set; } = true;
}
// TODO: Implement multi-select options in MiraAPI by using flags rather than enums.
/*[Flags]
public enum ValidBites : uint
{
    None = 0,
    NeutralBenign = 1,
    NeutralEvil = 2,
    NeutralOutlier = 3,
    Lovers = 8,
    All = 10,
}*/

public enum ValidBites
{
    OnlyCrew,
    NeutralBenign,
    NeutralEvil,
    NeutralOutlier,
    BenignAndEvil,
    BenignAndOutlier,
    EvilAndOutlier,
    NonKillerNeutrals,
}
