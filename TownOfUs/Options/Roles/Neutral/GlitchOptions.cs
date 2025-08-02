using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class GlitchOptions : AbstractOptionGroup<GlitchRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Glitch, "Glitch");

    [ModdedNumberOption("Kill Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("Mimic Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MimicCooldown { get; set; } = 25f;

    [ModdedNumberOption("Mimic Duration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MimicDuration { get; set; } = 10f;

    [ModdedToggleOption("Move While Using Mimic Menu (KB ONLY)")]
    public bool MoveWithMenu { get; set; } = true;
    
    [ModdedNumberOption("Hack Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HackCooldown { get; set; } = 25f;

    [ModdedNumberOption("Hack Duration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HackDuration { get; set; } = 10f;

    [ModdedToggleOption("Glitch Can Vent")]
    public bool CanVent { get; set; } = true;
}