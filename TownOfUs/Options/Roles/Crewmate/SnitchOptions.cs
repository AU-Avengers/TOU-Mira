using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SnitchOptions : AbstractRoleOptionGroup<SnitchRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Snitch", "Snitch");

    [ModdedToggleOption("TouOptionSnitchRevealsNeutralKillers")]
    public bool SnitchNeutralRoles { get; set; } = false;

    [ModdedToggleOption("TouOptionSnitchSeesTraitor")]
    public bool SnitchSeesTraitor { get; set; } = true;

    [ModdedToggleOption("TouOptionSnitchSeesImpostorsInMeetings")]
    public bool SnitchSeesImpostorsMeetings { get; set; } = true;

    [ModdedNumberOption("TouOptionSnitchTasksRemainingWhenRevealed", 1, 3)]
    public float TaskRemainingWhenRevealed { get; set; } = 1;
}