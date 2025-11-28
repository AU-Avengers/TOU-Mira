using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class InvestigatorOptions : AbstractOptionGroup<InvestigatorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleInvestigator", "Investigator");

    [ModdedNumberOption("TouOptionInvestigatorFootprintSize", 1f, 10f, suffixType: MiraNumberSuffixes.Multiplier)]
    public float FootprintSize { get; set; } = 4f;

    [ModdedNumberOption("TouOptionInvestigatorFootprintInterval", 0.5f, 6f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FootprintInterval { get; set; } = 1;

    [ModdedNumberOption("TouOptionInvestigatorFootprintDuration", 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FootprintDuration { get; set; } = 10f;

    [ModdedToggleOption("TouOptionInvestigatorShowAnonymousFootprints")]
    public bool ShowAnonymousFootprints { get; set; } = false;

    [ModdedToggleOption("TouOptionInvestigatorShowFootprintVent")]
    public bool ShowFootprintVent { get; set; } = false;
}