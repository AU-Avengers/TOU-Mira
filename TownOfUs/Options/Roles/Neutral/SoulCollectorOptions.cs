using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SoulCollectorOptions : AbstractOptionGroup<SoulCollectorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSoulCollector", "Soul Collector");

    [ModdedNumberOption("TouOptionSoulCollectorReapCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionSoulCollectorCanVent")]
    public bool CanVent { get; set; } = false;
}