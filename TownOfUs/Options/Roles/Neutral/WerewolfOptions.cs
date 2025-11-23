using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class WerewolfOptions : AbstractOptionGroup<WerewolfRole>
{
    public override string GroupName => TouLocale.Get("TouRoleWerewolf", "Werewolf");

    [ModdedNumberOption("TouOptionWerewolfRampageCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RampageCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionWerewolfRampageDuration", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RampageDuration { get; set; } = 10f;

    [ModdedNumberOption("TouOptionWerewolfKillCooldown", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float RampageKillCooldown { get; set; } = 1.5f;

    [ModdedToggleOption("TouOptionWerewolfCanVent")]
    public bool CanVent { get; set; } = true;
}