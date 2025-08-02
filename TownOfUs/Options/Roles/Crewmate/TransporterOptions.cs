using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TransporterOptions : AbstractOptionGroup<TransporterRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Transporter, "Transporter");

    [ModdedNumberOption("Transport Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TransporterCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Uses", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumTransports { get; set; } = 5f;

    [ModdedToggleOption("Move While Using Transport Menu (KB ONLY)")]
    public bool MoveWithMenu { get; set; } = true;

    [ModdedToggleOption("Can Use Vitals")]
    public bool CanUseVitals { get; set; } = true;

    [ModdedToggleOption("Get More Uses From Completing Tasks")]
    public bool TaskUses { get; set; } = true;
}