using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Assailant;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Assailant;

public sealed class OverclockerOptions : AbstractTouModifierOptionGroup<OverclockerModifier>
{
    public override string GroupName => MiraLocaleManager.Get("TouModifierOverclocker", "Overclocker");
    public override Color GroupColor => TownOfUsColors.Overclocker;

    public ModdedNumberOption OverclockCooldown { get; set; } = new("Overclock Cooldown", 5f, 5f, 120f, 2.5f,
        MiraNumberSuffixes.Seconds, formatString: "0.0");

    public ModdedNumberOption OverclockDuration { get; set; } = new("Overclock Duration", 50f, 20f, 120f, 2.5f,
        MiraNumberSuffixes.Seconds, formatString: "0.0");

    public ModdedToggleOption OverclockRoundOne { get; set; } = new("Allow Overclock in First Round", false);
    
    public ModdedNumberOption OverclockUses { get; set; } = new("Overclock Uses", 2, 1, 5, 1,
        MiraNumberSuffixes.None);

    public ModdedNumberOption OverclockMultiplier { get; set; } = new("Overclock Cooldown Multiplier", 2f, 1.1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption UnderclockMultiplier { get; set; } = new("Underclock Cooldown Multiplier", 0.5f, 0.2f, 0.9f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");
}