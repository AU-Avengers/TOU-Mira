using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class AmbusherOptions : AbstractOptionGroup<AmbusherRole>
{
    public override string GroupName => TouLocale.Get("TouRoleAmbusher", "Ambusher");

    [ModdedNumberOption("TouOptionAmbusherAmbushUsesPerGame", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxAmbushes { get; set; } = 0f;

    [ModdedNumberOption("TouOptionAmbusherAmbushCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AmbushCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionAmbusherPursueArrowUpdateInterval", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float UpdateInterval { get; set; } = 2.5f;

    [ModdedToggleOption("TouOptionAmbusherStopPursuingPlayerOnAmbush")]
    public bool ResetAmbush { get; set; } = true;

    [ModdedToggleOption("TouOptionAmbusherCanVent")]
    public bool CanVent { get; set; } = true;
}