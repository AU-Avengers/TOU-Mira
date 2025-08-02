using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class ScientistOptions : AbstractOptionGroup<ScientistModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Scientist, "Scientist");
    public override uint GroupPriority => 37;
    public override Color GroupColor => TownOfUsColors.Scientist;

    [ModdedToggleOption("Move While Using Vitals")]
    public bool MoveWithMenu { get; set; } = true;

    [ModdedNumberOption("Starting Charge", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float StartingCharge { get; set; } = 20f;

    [ModdedNumberOption("Battery Charged Each Round", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RoundCharge { get; set; } = 15f;

    [ModdedNumberOption("Battery Charged Per Task", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TaskCharge { get; set; } = 10f;

    [ModdedNumberOption("Vitals Display Cooldown", 0f, 30f, 5f, MiraNumberSuffixes.Seconds)]
    public float DisplayCooldown { get; set; } = 15f;

    [ModdedNumberOption("Max Vitals Display Duration", 0f, 30f, 5f, MiraNumberSuffixes.Seconds, zeroInfinity: true)]
    public float DisplayDuration { get; set; } = 15f;
}