using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class ScavengerOptions : AbstractOptionGroup<ScavengerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Scavenger, "Scavenger");

    [ModdedNumberOption("Scavenge Duration", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ScavengeDuration { get; set; } = 25f;

    [ModdedNumberOption("Scavenge Duration Increase Per Kill", 5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ScavengeIncreaseDuration { get; set; } = 10f;

    [ModdedNumberOption("Scavenge Kill Cooldown On Correct Kill", 5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ScavengeCorrectKillCooldown { get; set; } = 10f;

    [ModdedNumberOption("Kill Cooldown Multiplier On Incorrect Kill", 1.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float ScavengeIncorrectKillCooldown { get; set; } = 3f;
}