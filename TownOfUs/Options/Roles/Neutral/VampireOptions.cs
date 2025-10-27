using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class VampireOptions : AbstractOptionGroup<VampireRole>
{
    public override string GroupName => TouLocale.Get("TouRoleVampire", "Vampire");

    [ModdedNumberOption("Bite Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BiteCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Number Of Vampires Per Game", 2, 5, 1, MiraNumberSuffixes.None, "0")]
    public float MaxVampires { get; set; } = 2;

    [ModdedToggleOption("Vampires Have Impostor Vision")]
    public bool HasVision { get; set; } = true;

    [ModdedToggleOption("New Vampires Can Assassinate")]
    public bool CanGuessAsNewVamp { get; set; } = true;

    public ModdedEnumOption<ValidBites> ValidConversions { get; } = new("Valid Neutral Conversions", ValidBites.BenignAndEvil,
        ["Only Crew", "Benign", "Evils", "Outliers", "Benign & Evil", "Benign & Outlier", "Evil & Outlier", "Non-Killer Neutrals"]);

    public ModdedToggleOption ConvertLovers { get; set; } = new("Can Convert Lovers", false);

    [ModdedToggleOption("New Vampires Can Convert")]
    public bool CanConvertAsNewVamp { get; set; } = true;

    [ModdedToggleOption("Vampires Can Vent")]
    public bool CanVent { get; set; } = true;
}

public enum ValidBites
{
    NeutralBenign,
    NeutralEvil,
    NeutralOutlier,
    BenignAndEvil,
    BenignAndOutlier,
    EvilAndOutlier,
    OnlyCrew,
    NonKillerNeutrals,
}
