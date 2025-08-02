using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class EclipsalOptions : AbstractOptionGroup<EclipsalRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Eclipsal, "Eclipsal");

    [ModdedNumberOption("Blind Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlindCooldown { get; set; } = 25f;

    [ModdedNumberOption("Blind Duration", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlindDuration { get; set; } = 15f;

    [ModdedNumberOption("Blind Radius", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float BlindRadius { get; set; } = 1f;
}