using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class WerewolfOptions : AbstractOptionGroup<WerewolfRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Werewolf, "Werewolf");

    [ModdedNumberOption("Rampage Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RampageCooldown { get; set; } = 25f;

    [ModdedNumberOption("Rampage Duration", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RampageDuration { get; set; } = 10f;

    [ModdedNumberOption("Rampage Kill Cooldown", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float RampageKillCooldown { get; set; } = 1.5f;

    [ModdedToggleOption("Werewolf Can Vent When Rampaged")]
    public bool CanVent { get; set; } = true;
}