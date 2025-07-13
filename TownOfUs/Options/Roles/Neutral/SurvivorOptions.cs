﻿using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SurvivorOptions : AbstractOptionGroup<SurvivorRole>
{
    public override string GroupName => "Survivor";

    [ModdedNumberOption("Vest Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VestCooldown { get; set; } = 25f;

    [ModdedNumberOption("Vest Duration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float VestDuration { get; set; } = 10f;

    [ModdedNumberOption("Max Number Of Vests", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxVests { get; set; } = 5f;

    [ModdedToggleOption("Survivor Scatter Mechanic Enabled")]
    public bool ScatterOn { get; set; } = true;

    public ModdedNumberOption ScatterTimer { get; set; } = new("Survivor Scatter Timer", 25f, 10f, 60f, 2.5f,
        MiraNumberSuffixes.Seconds, "0.0")
    {
        Visible = () => !OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn
    };
}