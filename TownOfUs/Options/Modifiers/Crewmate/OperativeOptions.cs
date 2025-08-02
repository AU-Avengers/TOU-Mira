using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class OperativeOptions : AbstractOptionGroup<OperativeModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Operative, "Operative");
    public override uint GroupPriority => 35;

    public override Color GroupColor => new(0.8f, 0.33f, 0.37f, 1f);

    // THESE BREAK THE CAMERA MINIGAME!!
/*
        [ModdedToggleOption("Move While Using Cameras")]
        public bool MoveWithCams { get; set; } = false;

        [ModdedToggleOption("Move While Using Fungle Binoculars")]
        public bool MoveOnFungle { get; set; } = false;
     */
    [ModdedToggleOption("Move While Using Mira Doorlog")]
    public bool MoveOnMira { get; set; } = true;

    [ModdedNumberOption("Starting Charge", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float StartingCharge { get; set; } = 20f;

    [ModdedNumberOption("Battery Charged Each Round", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RoundCharge { get; set; } = 10f;

    [ModdedNumberOption("Battery Charged Per Task", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TaskCharge { get; set; } = 7.5f;

    [ModdedNumberOption("Security Display Cooldown", 0f, 30f, 5f, MiraNumberSuffixes.Seconds)]
    public float DisplayCooldown { get; set; } = 15f;

    [ModdedNumberOption("Max Security Display Duration", 0f, 30f, 5f, MiraNumberSuffixes.Seconds, zeroInfinity: true)]
    public float DisplayDuration { get; set; } = 15f;
}