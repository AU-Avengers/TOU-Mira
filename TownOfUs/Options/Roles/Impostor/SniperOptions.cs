using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class SniperOptions : AbstractOptionGroup<SniperRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Sniper, "Sniper");

    [ModdedNumberOption("Shoot Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ShootCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Shots", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public int MaxShots { get; set; } = 2;

    [ModdedToggleOption("Reveal Sniper Audio")]
    public bool RevealSniper { get; set; } = true;

    [ModdedNumberOption("Aim Duration", 2f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float AimDuration { get; set; } = 5f;
}

