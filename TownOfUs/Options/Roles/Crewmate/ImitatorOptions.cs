using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ImitatorOptions : AbstractOptionGroup<ImitatorRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Imitator, "Imitator");

    [ModdedToggleOption("Imitate Specific Neutrals As Similar Crew Roles")]
    public bool ImitateNeutrals { get; set; } = true;

    [ModdedToggleOption("Imitate Specific Impostors As Similar Crew Roles")]
    public bool ImitateImpostors { get; set; } = true;
}