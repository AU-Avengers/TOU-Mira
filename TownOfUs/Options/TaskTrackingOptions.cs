using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace TownOfUs.Options;

public sealed class TaskTrackingOptions : AbstractOptionGroup
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Task Tracking";
    public override uint GroupPriority => 4;

    [ModdedToggleOption("See Tasks During Round")]
    public bool ShowTaskRound { get; set; } = true;

    [ModdedToggleOption("See Tasks During Meetings")]
    public bool ShowTaskInMeetings { get; set; } = true;

    [ModdedToggleOption("See Tasks When Dead")]
    public bool ShowTaskDead { get; set; } = true;
}