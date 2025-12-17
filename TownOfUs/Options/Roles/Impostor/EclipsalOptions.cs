using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class EclipsalOptions : AbstractOptionGroup<EclipsalRole>
{
    public override string GroupName => TouLocale.Get("TouRoleEclipsal", "Eclipsal");

    [ModdedNumberOption("TouOptionEclipsalBlindCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlindCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionEclipsalBlindDuration", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlindDuration { get; set; } = 15f;

    [ModdedNumberOption("TouOptionEclipsalBlindRadius", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float BlindRadius { get; set; } = 1f;
}