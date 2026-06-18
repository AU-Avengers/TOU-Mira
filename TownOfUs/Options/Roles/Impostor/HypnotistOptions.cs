using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class HypnotistOptions : AbstractOptionGroup<HypnotistRole>
{
    public override string GroupName => TouLocale.Get("TouRoleHypnotist", "Hypnotist");

    [ModdedNumberOption("Hypnotize Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HypnotiseCooldown { get; set; } = 25f;

    [ModdedNumberOption("Amount of Rounds Hysteria Lasts", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float HysteriaRoundDuration { get; set; } = 0f;

    [ModdedToggleOption("Hypnotist Can Kill With Teammate")]
    public bool HypnoKill { get; set; } = true;
}