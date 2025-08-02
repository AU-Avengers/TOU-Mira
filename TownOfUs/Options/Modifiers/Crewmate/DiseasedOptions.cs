using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class DiseasedOptions : AbstractOptionGroup<DiseasedModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Diseased, "Diseased");
    public override uint GroupPriority => 32;
    public override Color GroupColor => TownOfUsColors.Diseased;

    [ModdedNumberOption("Diseased Kill Multiplier", 1.5f, 5f, 0.5f, MiraNumberSuffixes.Multiplier)]
    public float CooldownMultiplier { get; set; } = 3f;
}