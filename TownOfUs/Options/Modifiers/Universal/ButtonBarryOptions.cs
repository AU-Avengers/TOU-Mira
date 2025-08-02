using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class ButtonBarryOptions : AbstractOptionGroup<ButtonBarryModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.ButtonBarry, "Button Barry");
    public override uint GroupPriority => 22;
    public override Color GroupColor => TownOfUsColors.ButtonBarry;

    [ModdedNumberOption("Button Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("Max Uses", 1f, 3f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumButtons { get; set; } = 1f;

    [ModdedToggleOption("Ignore Sabotages")]
    public bool IgnoreSabo { get; set; } = true;

    [ModdedToggleOption("Allow Usage in First Round")]
    public bool FirstRoundUse { get; set; } = false;
}