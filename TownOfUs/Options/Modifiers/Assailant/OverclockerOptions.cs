using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Assailant;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Assailant;

public sealed class OverclockerOptions : AbstractTouModifierOptionGroup<OverclockerModifier>
{
    public override string GroupName => TouLocale.Get("TouModifierOverclocker", "Overclocker");
    public override Color GroupColor => TownOfUsColors.Overclocker;

    public ModdedNumberOption OverclockCooldown { get; set; } =
        new("TouOptionOverclockerCooldown", 5f, 5f, 120f, 2.5f,
            MiraNumberSuffixes.Seconds, formatString: "0.0");

    public ModdedNumberOption OverclockDuration { get; set; } =
        new("TouOptionOverclockerDuration", 50f, 20f, 120f, 2.5f,
            MiraNumberSuffixes.Seconds, formatString: "0.0");

    public ModdedToggleOption OverclockRoundOne { get; set; } =
        new("TouOptionOverclockerRoundOne", false);

    public ModdedNumberOption OverclockUses { get; set; } =
        new("TouOptionOverclockerUses", 2, 1, 5, 1,
            MiraNumberSuffixes.None);

    public ModdedNumberOption OverclockMultiplier { get; set; } =
        new("TouOptionOverclockerMultiplier", 2f, 1.1f, 3f, 0.1f,
            MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption UnderclockMultiplier { get; set; } =
        new("TouOptionOverclockerUnderclockMultiplier", 0.5f, 0.2f, 0.9f, 0.1f,
            MiraNumberSuffixes.Multiplier, "0.00");
}