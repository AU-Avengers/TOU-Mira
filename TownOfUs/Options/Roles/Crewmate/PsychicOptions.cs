using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PsychicOptions : AbstractOptionGroup<PsychicRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Psychic, "Psychic");

    [ModdedNumberOption("Vision Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PsychicCooldown { get; set; } = 25f;
    [ModdedToggleOption("Merge Impostor Concealing/Support")]
    public bool UseCommonImpostor { get; set; } = false;
    [ModdedToggleOption("Merge Impostor Killing/Power")]
    public bool UseSpecialImpostor { get; set; } = false;
    [ModdedToggleOption("Merge Neutral Benign/Evil")]
    public bool UseCommonNeutral { get; set; } = false;
}