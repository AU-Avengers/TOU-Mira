using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class NoisemakerOptions : AbstractOptionGroup<NoisemakerModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Noisemaker, "Noisemaker");
    public override uint GroupPriority => 34;
    public override Color GroupColor => TownOfUsColors.Noisemaker;

    [ModdedToggleOption("Impostors Get Alert")]
    public bool ImpostorsAlerted { get; set; } = true;

    [ModdedToggleOption("Neutral Killers Get Alert")]
    public bool NeutsAlerted { get; set; } = true;

    [ModdedToggleOption("Comms Sabotage Prevents Alert")]
    public bool CommsAffected { get; set; } = false;

    [ModdedToggleOption("Only Triggers If A Body Exists")]
    public bool BodyCheck { get; set; } = true;

    [ModdedNumberOption("Alert Duration", 1f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float AlertDuration { get; set; } = 5f;
}