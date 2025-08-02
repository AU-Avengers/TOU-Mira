using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SurvivorOptions : AbstractOptionGroup<SurvivorRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Survivor, "Survivor");

    [ModdedNumberOption("Vest Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VestCooldown { get; set; } = 25f;

    [ModdedNumberOption("Vest Duration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float VestDuration { get; set; } = 10f;

    [ModdedNumberOption("Max Number Of Vests", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxVests { get; set; } = 10f;

    [ModdedToggleOption("Survivor Scatter Mechanic Enabled")]
    public bool ScatterOn { get; set; } = false;

    [ModdedNumberOption("Survivor Scatter Timer", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float ScatterTimer { get; set; } = 25f;
}