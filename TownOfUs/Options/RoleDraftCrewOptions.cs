using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class RoleDraftCrewOptions : AbstractOptionGroup
{
    public override Func<bool> GroupVisible => () =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft &&
        !OptionGroupSingleton<RoleOptions>.Instance.UseRoleListForPool;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;

    public override string GroupName => "Crewmate Settings";
    public override uint GroupPriority => 2;

    public ModdedNumberOption MaxCrewInvestigative { get; set; } =
        new("Max Investigative Roles", 5f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewKilling { get; set; } =
        new("Max Killing Roles", 3f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewPower { get; set; } =
        new("Max Power Roles", 2f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewProtective { get; set; } =
        new("Max Protective Roles", 2f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewSupport { get; set; } =
        new("Max Support Roles", 3f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");
}