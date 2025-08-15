using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class PredatorOptions : AbstractOptionGroup<PredatorRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Predator, "Predator");

    [ModdedNumberOption("Predator Kill Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PredatorKillCooldown { get; set; } = 25f;

    [ModdedNumberOption("Predator Stare Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float PredatorStareCooldown { get; set; } = 30f;

    [ModdedNumberOption("Predator Stare Duration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PredatorStareDuration { get; set; } = 20f;

    [ModdedNumberOption("Max Stare Uses", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float StareUses { get; set; } = 5;
}