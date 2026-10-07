using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace TownOfUs.Options;

public sealed class RoleExclusionOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.RoleExclusions");
    public override uint GroupPriority => 2;
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;

    [ModdedToggleOption("TouOptionExclusiveCleaningRoles")]
    public bool CleaningRoles { get; set; } = false;

    [ModdedToggleOption("TouOptionExclusiveFramingRoles")]
    public bool FramingRoles { get; set; } = false;

    [ModdedToggleOption("TouOptionExclusiveRevivingRoles")]
    public bool RevivingRoles { get; set; } = false;

    [ModdedToggleOption("TouOptionExclusiveBlindingRoles")]
    public bool BlindingRoles { get; set; } = false;

    [ModdedToggleOption("TouOptionExclusiveMunicipalRoles")]
    public bool MunicipalRoles { get; set; } = false;
}
