using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class VampireOptions : AbstractOptionGroup<VampireRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Vampire, "Vampire");

    [ModdedNumberOption("Bite Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BiteCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Number Of Vampires Per Game", 2, 5, 1, MiraNumberSuffixes.None, "0")]
    public float MaxVampires { get; set; } = 2;

    [ModdedToggleOption("Vampires Have Impostor Vision")]
    public bool HasVision { get; set; } = true;

    [ModdedToggleOption("New Vampires Can Assassinate")]
    public bool CanGuessAsNewVamp { get; set; } = true;

    [ModdedEnumOption("Valid Conversions", typeof(BiteOptions),
    [
        "Crewmates", "Crew & NBs", "Crew & NEs", "Crew, NBs & NEs", "Crew & Lovers", "Crew, Lovers & NBs",
        "Crew, Lovers & NEs", "Crew, Lovers, NBs & NEs"
    ])]
    public BiteOptions ConvertOptions { get; set; } = BiteOptions.CrewNeutralBenignAndNeutralEvil;

    [ModdedToggleOption("New Vampires Can Convert")]
    public bool CanConvertAsNewVamp { get; set; } = true;

    [ModdedToggleOption("Vampires Can Vent")]
    public bool CanVent { get; set; } = true;
}

public enum BiteOptions
{
    OnlyCrewmates,
    CrewAndNeutralBenign,
    CrewAndNeutralEvil,
    CrewNeutralBenignAndNeutralEvil,
    CrewAndLovers,
    CrewLoversAndNeutralBenign,
    CrewLoversAndNeutralEvil,
    CrewLoversNeutralBenignAndNeutralEvil
}