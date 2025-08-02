using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class PlaguebearerOptions : AbstractOptionGroup<PlaguebearerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Plaguebearer, "Plaguebearer");

    [ModdedNumberOption("Instant Pestilence Chance", 0, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float PestChance { get; set; } = 0f;

    [ModdedNumberOption("Infect Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InfectCooldown { get; set; } = 25f;

    [ModdedToggleOption("Announce Pestilence Transformation")]
    public bool AnnouncePest { get; set; } = true;

    [ModdedNumberOption("Pestilence Kill Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PestKillCooldown { get; set; } = 25f;

    [ModdedToggleOption("Pestilence Can Vent")]
    public bool CanVent { get; set; } = false;
}