using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class AurialOptions : AbstractOptionGroup<AurialRole>
{
    public override string GroupName => TouLocale.Get("TouRoleAurial", "Aurial");

    [ModdedNumberOption("TouOptionAurialAuraInnerRadius", 0f, 1f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float AuraInnerRadius { get; set; } = 0.5f;

    [ModdedNumberOption("TouOptionAurialAuraOuterRadius", 1f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float AuraOuterRadius { get; set; } = 1.5f;

    [ModdedNumberOption("TouOptionAurialSenseDuration", 1f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float SenseDuration { get; set; } = 10f;
}