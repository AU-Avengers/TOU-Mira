using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TransporterOptions : AbstractOptionGroup<TransporterRole>
{
    public override string GroupName => TouLocale.Get("TouRoleTransporter", "Transporter");

    [ModdedNumberOption("TouOptionTransporterTransportCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TransporterCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionTransporterMaxUses", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumTransports { get; set; } = 5f;

    [ModdedToggleOption("TouOptionTransporterMoveWithMenu")]
    public bool MoveWithMenu { get; set; } = true;

    [ModdedToggleOption("TouOptionTransporterCanUseVitals")]
    public bool CanUseVitals { get; set; } = true;

    [ModdedToggleOption("TouOptionTransporterGetUsesFromTasks")]
    public bool TaskUses { get; set; } = true;
}