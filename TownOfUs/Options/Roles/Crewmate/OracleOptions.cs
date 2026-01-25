using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class OracleOptions : AbstractOptionGroup<OracleRole>
{
    public override string GroupName => TouLocale.Get("TouRoleOracle", "Oracle");

    [ModdedNumberOption("TouOptionOracleConfessCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float ConfessCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionOracleBlessCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlessCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionOracleRevealAccuracy", 0f, 100f, suffixType: MiraNumberSuffixes.Percent)]
    public float RevealAccuracyPercentage { get; set; } = 80f;

    [ModdedToggleOption("TouOptionOracleNeutralBenignShowEvil")]
    public bool ShowNeutralBenignAsEvil { get; set; } = false;

    [ModdedToggleOption("TouOptionOracleNeutralEvilShowEvil")]
    public bool ShowNeutralEvilAsEvil { get; set; } = false;

    [ModdedToggleOption("TouOptionOracleNeutralKillingShowEvil")]
    public bool ShowNeutralKillingAsEvil { get; set; } = true;

    [ModdedToggleOption("TouOptionOracleNeutralOutlierShowEvil")]
    public bool ShowNeutralOutlierAsEvil { get; set; } = true;
}