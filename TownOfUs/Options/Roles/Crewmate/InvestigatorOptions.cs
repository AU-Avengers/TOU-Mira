using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class InvestigatorOptions : AbstractOptionGroup<InvestigatorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleInvestigator", "Investigator");

    public ModdedEnumOption FootprintMode { get; set; } = new("TouOptionInvestigatorFootprintSeperated", (int)PrintMode.Distance,
        typeof(PrintMode));

    public ModdedNumberOption FootprintIntervalDistance { get; set; } = new("TouOptionInvestigatorFootprintInterval",
        0.5f, 0.25f, 3f, 0.5f, MiraNumberSuffixes.None)
    {
        Visible = () => (PrintMode)OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintMode.Value is PrintMode.Distance
    };

    public ModdedNumberOption FootprintIntervalTime { get; set; } = new("TouOptionInvestigatorFootprintInterval",
        1, 0.5f, 6f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => (PrintMode)OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintMode.Value is PrintMode.Time
    };
    public float FootprintInterval => (PrintMode)FootprintMode.Value is PrintMode.Distance ? FootprintIntervalDistance.Value : FootprintIntervalTime.Value;

    [ModdedNumberOption("TouOptionInvestigatorFootprintSize", 1f, 10f, suffixType: MiraNumberSuffixes.Multiplier)]
    public float FootprintSize { get; set; } = 4f;

    [ModdedNumberOption("TouOptionInvestigatorFootprintDuration", 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FootprintDuration { get; set; } = 10f;

    [ModdedToggleOption("TouOptionInvestigatorShowAnonymousFootprints")]
    public bool ShowAnonymousFootprints { get; set; } = false;

    [ModdedToggleOption("TouOptionInvestigatorShowFootprintVent")]
    public bool ShowFootprintVent { get; set; } = false;
}

public enum PrintMode
{
    Distance,
    Time
}