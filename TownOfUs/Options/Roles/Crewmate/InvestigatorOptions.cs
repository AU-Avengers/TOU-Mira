﻿using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class InvestigatorOptions : AbstractOptionGroup<InvestigatorRole>
{
    public override string GroupName => "Investigator";

    [ModdedNumberOption("Footprint Size", 1f, 10f, suffixType: MiraNumberSuffixes.Multiplier)]
    public float FootprintSize { get; set; } = 4f;

    [ModdedNumberOption("Footprint Interval", 0.05f, 1f, 0.05f, MiraNumberSuffixes.Seconds, "0.00")]
    public float FootprintInterval { get; set; } = 0.1f;

    [ModdedNumberOption("Footprint Duration", 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FootprintDuration { get; set; } = 10f;

    [ModdedToggleOption("Anonymous Footprint")]
    public bool ShowAnonymousFootprints { get; set; } = false;

    [ModdedToggleOption("Footprint Vent Visible")]
    public bool ShowFootprintVent { get; set; } = false;
}