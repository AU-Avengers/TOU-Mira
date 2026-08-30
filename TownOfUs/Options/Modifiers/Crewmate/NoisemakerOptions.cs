using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class NoisemakerOptions : AbstractTouModifierOptionGroup<NoisemakerModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierNoisemaker", "Noisemaker");
    public override uint GroupPriority => 23;
    public override Color GroupColor => TownOfUsColors.Noisemaker;

    [ModdedToggleOption("TouOptionNoisemakerImpostorsAlerted")]
    public bool ImpostorsAlerted { get; set; } = true;

    [ModdedToggleOption("TouOptionNoisemakerNeutsAlerted")]
    public bool NeutsAlerted { get; set; } = true;

    [ModdedToggleOption("TouOptionNoisemakerCommsAffected")]
    public bool CommsAffected { get; set; } = false;

    [ModdedToggleOption("TouOptionNoisemakerBodyCheck")]
    public bool BodyCheck { get; set; } = true;

    [ModdedNumberOption("TouOptionNoisemakerAlertDuration", 1f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float AlertDuration { get; set; } = 5f;
}