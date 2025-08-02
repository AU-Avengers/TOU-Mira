using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class FrostyOptions : AbstractOptionGroup<FrostyModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Frosty, "Frosty");
    public override uint GroupPriority => 33;
    public override Color GroupColor => TownOfUsColors.Frosty;

    [ModdedNumberOption("Chill Duration", 0f, 15f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ChillDuration { get; set; } = 10f;

    [ModdedNumberOption("Chill Start Speed", 0.25f, 0.95f, 0.05f, MiraNumberSuffixes.Multiplier)]
    public float ChillStartSpeed { get; set; } = 0.75f;
}