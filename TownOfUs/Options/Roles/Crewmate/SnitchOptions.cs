using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SnitchOptions : AbstractOptionGroup<SnitchRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Snitch, "Snitch");

    [ModdedToggleOption("Snitch Reveals Neutral Killers")]
    public bool SnitchNeutralRoles { get; set; } = false;

    [ModdedToggleOption("Snitch Sees Traitor")]
    public bool SnitchSeesTraitor { get; set; } = true;

    [ModdedToggleOption("Snitch Sees Impostors In Meetings")]
    public bool SnitchSeesImpostorsMeetings { get; set; } = true;

    [ModdedNumberOption("Tasks Remaining When Revealed", 1, 3)]
    public float TaskRemainingWhenRevealed { get; set; } = 1;
}