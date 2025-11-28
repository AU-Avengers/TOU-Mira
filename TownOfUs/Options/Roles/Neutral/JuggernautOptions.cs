using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class JuggernautOptions : AbstractOptionGroup<JuggernautRole>
{
    public override string GroupName => TouLocale.Get("TouRoleJuggernaut", "Juggernaut");

    [ModdedNumberOption("TouOptionJuggernautInitialCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionJuggernautCooldownReduction", 2.5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldownReduction { get; set; } = 5f;

    [ModdedToggleOption("TouOptionJuggernautCanVent")]
    public bool CanVent { get; set; } = true;
}