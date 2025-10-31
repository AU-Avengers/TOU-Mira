using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ImitatorOptions : AbstractOptionGroup<ImitatorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleImitator", "Imitator");

    public ModdedToggleOption ImitateNeutrals { get; set; } = new("Imitate Specific Neutrals As Similar Crew Roles", true);

    public ModdedToggleOption ImitateImpostors { get; set; } = new("Imitate Specific Impostors As Similar Crew Roles", true);

    public ModdedToggleOption ImitateBasicCrewmate { get; set; } = new("Imitate Basic Crewmate", true);
}