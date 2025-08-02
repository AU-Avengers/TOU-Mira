using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class ShyOptions : AbstractOptionGroup<ShyModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Shy, "Shy");
    public override uint GroupPriority => 28;
    public override Color GroupColor => TownOfUsColors.Shy;

    [ModdedNumberOption("Transparency Delay", 0f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float InvisDelay { get; set; } = 5f;

    [ModdedNumberOption("Turn Transparent Duration", 0f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float TransformInvisDuration { get; set; } = 5f;

    [ModdedNumberOption("Final Opacity", 0f, 80f, 10f, MiraNumberSuffixes.Percent)]
    public float FinalTransparency { get; set; } = 20f;
}