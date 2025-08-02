using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ClericOptions : AbstractOptionGroup<ClericRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Cleric, "Cleric");

    [ModdedNumberOption("Barrier Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float BarrierCooldown { get; set; } = 25f;

    [ModdedNumberOption("Barrier Duration", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float BarrierDuration { get; set; } = 25f;

    [ModdedNumberOption("Cleanse Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float CleanseCooldown { get; set; } = 25f;

    [ModdedEnumOption("Show Barriered Player", typeof(BarrierOptions), ["Barriered", "Cleric", "Barriered + Cleric"])]
    public BarrierOptions ShowBarriered { get; set; } = BarrierOptions.Cleric;

    [ModdedToggleOption("Cleric Gets Attack Notification")]
    public bool AttackNotif { get; set; } = true;
}

public enum BarrierOptions
{
    Self,
    Cleric,
    SelfAndCleric
}