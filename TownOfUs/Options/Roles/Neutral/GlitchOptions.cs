using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class GlitchOptions : AbstractOptionGroup<GlitchRole>
{
    public override string GroupName => TouLocale.Get("TouRoleGlitch", "Glitch");

    [ModdedNumberOption("TouOptionGlitchKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionGlitchMimicCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MimicCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionGlitchMimicDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MimicDuration { get; set; } = 10f;

    [ModdedToggleOption("TouOptionGlitchMoveInMimicMenu")]
    public bool MoveWithMenu { get; set; } = true;

    [ModdedNumberOption("TouOptionGlitchHackCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HackCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionGlitchHackDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HackDuration { get; set; } = 10f;

    [ModdedToggleOption("TouOptionGlitchCan Vent")]
    public bool CanVent { get; set; } = true;
}