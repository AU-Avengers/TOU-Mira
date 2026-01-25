using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SurvivorOptions : AbstractOptionGroup<SurvivorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSurvivor", "Survivor");

    [ModdedNumberOption("TouOptionSurvivorVestCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VestCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionSurvivorVestDuration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float VestDuration { get; set; } = 10f;

    [ModdedNumberOption("TouOptionSurvivorMaxVests", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxVests { get; set; } = 10f;

    [ModdedToggleOption("TouOptionSurvivorScatterEnabled")]
    public bool ScatterOn { get; set; } = false;

    [ModdedNumberOption("TouOptionSurvivorScatterTimer", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float ScatterTimer { get; set; } = 25f;
}