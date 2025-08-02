using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class VeteranOptions : AbstractOptionGroup<VeteranRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Veteran, "Veteran");

    [ModdedNumberOption("Alert Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AlertCooldown { get; set; } = 25f;

    [ModdedNumberOption("Alert Duration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AlertDuration { get; set; } = 10f;

    [ModdedNumberOption("Max Number of Alerts", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumAlerts { get; set; } = 5f;

    [ModdedToggleOption("Can Be Killed On Alert")]
    public bool KilledOnAlert { get; set; } = false;

    [ModdedToggleOption("Get More Uses From Completing Tasks")]
    public bool TaskUses { get; set; } = true;
}