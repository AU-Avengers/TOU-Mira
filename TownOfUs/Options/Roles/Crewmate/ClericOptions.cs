using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ClericOptions : AbstractOptionGroup<ClericRole>
{
    public override string GroupName => TouLocale.Get("TouRoleCleric", "Cleric");

    [ModdedNumberOption("TouOptionClericBarrierCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float BarrierCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionClericBarrierDuration", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float BarrierDuration { get; set; } = 25f;

    [ModdedNumberOption("TouOptionClericCleanseCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float CleanseCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionClericProtectedSeesBarrier")]
    public bool ShowBarrier { get; set; } = false;

    [ModdedToggleOption("TouOptionClericAttackNotif")]
    public bool AttackNotif { get; set; } = true;
}