using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ForensicOptions : AbstractOptionGroup<ForensicRole>
{
    public override string GroupName => TouLocale.Get("TouRoleForensic", "Forensic");

    [ModdedNumberOption("Examine Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ExamineCooldown { get; set; } = 25f;

    [ModdedToggleOption("Show Forensic Reports")]
    public bool ForensicReportOn { get; set; } = true;

    public ModdedNumberOption ForensicRoleDuration { get; set; } = new("Time Where Forensic Will Have Role", 7.5f, 0f,
        60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<ForensicOptions>.Instance.ForensicReportOn
    };

    public ModdedNumberOption ForensicFactionDuration { get; set; } = new("Time Where Forensic Will Have Faction",
        30f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<ForensicOptions>.Instance.ForensicReportOn
    };
}