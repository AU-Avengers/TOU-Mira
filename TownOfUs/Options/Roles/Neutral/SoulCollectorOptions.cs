using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SoulCollectorOptions : AbstractOptionGroup<SoulCollectorRole>
{
    public override string GroupName => TouLocale.Get(TouNames.SoulCollector, "Soul Collector");

    [ModdedNumberOption("Reap Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("Soul Collector Can Vent")]
    public bool CanVent { get; set; } = false;
}