using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class FlashOptions : AbstractOptionGroup<FlashModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Flash, "Flash");
    public override uint GroupPriority => 24;
    public override Color GroupColor => TownOfUsColors.Flash;

    [ModdedNumberOption("Flash Speed", 1.05f, 2.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float FlashSpeed { get; set; } = 1.75f;
}