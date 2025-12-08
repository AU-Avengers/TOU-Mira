using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class JuggernautOptions : AbstractOptionGroup<JuggernautRole>
{
    public override string GroupName => TouLocale.Get("TouRoleJuggernaut", "Juggernaut");

    [ModdedNumberOption("TouOptionJuggernautInitialCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    public ModdedNumberOption KillCooldownReduction { get; } = new(
        "TouOptionJuggernautCooldownReduction", 
        5f, 
        2.5f,
        15f, 
        1f, 
        MiraNumberSuffixes.Seconds, 
        formatString: null, 
        zeroInfinity: false, 
        includeInPreset: true)
    {
        // Note: halfIncrements functionality not available in runtime version
    };

    [ModdedToggleOption("TouOptionJuggernautCanVent")]
    public bool CanVent { get; set; } = true;
}