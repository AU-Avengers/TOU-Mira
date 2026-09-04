using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class JuggernautOptions : AbstractRoleOptionGroup<JuggernautRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Juggernaut", "Juggernaut");

    [ModdedNumberOption("TouOptionJuggernautInitialCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    public ModdedNumberOption KillCooldownReduction { get; } = new("TouOptionJuggernautCooldownReduction", 5f, 2.5f,
        15f, 1f, "#", "#", MiraNumberSuffixes.Seconds, halfIncrements: true);

    [ModdedToggleOption("TouOptionJuggernautCanVent")]
    public bool CanVent { get; set; } = true;
}