using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class DetectiveOptions : AbstractOptionGroup<DetectiveRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Detective, "Detective");

    [ModdedNumberOption("Examine Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ExamineCooldown { get; set; } = 25f;

    [ModdedToggleOption("Show Detective Reports")]
    public bool DetectiveReportOn { get; set; } = true;

    public ModdedNumberOption DetectiveRoleDuration { get; set; } = new("Time Where Detective Will Have Role", 7.5f, 0f,
        60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<DetectiveOptions>.Instance.DetectiveReportOn
    };

    public ModdedNumberOption DetectiveFactionDuration { get; set; } = new("Time Where Detective Will Have Faction",
        30f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<DetectiveOptions>.Instance.DetectiveReportOn
    };
}