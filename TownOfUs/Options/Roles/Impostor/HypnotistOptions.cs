using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class HypnotistOptions : AbstractOptionGroup<HypnotistRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Hypnotist, "Hypnotist");

    [ModdedNumberOption("Hypnotize Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HypnotiseCooldown { get; set; } = 25f;

    [ModdedToggleOption("Hypnotist Can Kill With Teammate")]
    public bool HypnoKill { get; set; } = true;
}