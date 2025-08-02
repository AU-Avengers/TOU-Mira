using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class VenererOptions : AbstractOptionGroup<VenererRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Venerer, "Venerer");

    [ModdedNumberOption("Ability Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AbilityCooldown { get; set; } = 25f;

    [ModdedNumberOption("Ability Duration", 5f, 15f, suffixType: MiraNumberSuffixes.Seconds)]
    public float AbilityDuration { get; set; } = 10f;

    [ModdedNumberOption("Sprint Speed", 1.05f, 2.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float NumSprintSpeed { get; set; } = 1.25f;

    [ModdedNumberOption("Minimum Freeze Speed", 0.05f, 0.75f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float MinFreezeSpeed { get; set; } = 0.25f;

    [ModdedNumberOption("Freeze Radius", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float FreezeRadius { get; set; } = 1f;
}