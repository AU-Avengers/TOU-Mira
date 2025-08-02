using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class OracleOptions : AbstractOptionGroup<OracleRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Oracle, "Oracle");

    [ModdedNumberOption("Confess Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float ConfessCooldown { get; set; } = 20f;

    [ModdedNumberOption("Bless Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlessCooldown { get; set; } = 25f;

    [ModdedNumberOption("Reveal Accuracy", 0f, 100f, suffixType: MiraNumberSuffixes.Percent)]
    public float RevealAccuracyPercentage { get; set; } = 80f;

    [ModdedToggleOption("Neutral Benign Show Up As Evil")]
    public bool ShowNeutralBenignAsEvil { get; set; } = false;

    [ModdedToggleOption("Neutral Evil Show Up As Evil")]
    public bool ShowNeutralEvilAsEvil { get; set; } = false;

    [ModdedToggleOption("Neutral Killing Show Up As Evil")]
    public bool ShowNeutralKillingAsEvil { get; set; } = true;
}