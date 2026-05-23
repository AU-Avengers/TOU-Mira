using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class RoleDraftRoleListOptions : AbstractOptionGroup
{
    public override Func<bool> GroupVisible => () =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft &&
        OptionGroupSingleton<RoleOptions>.Instance.UseRoleListForPool;

    public override string GroupName => "Role List Settings";
    public override uint GroupPriority => 3;
    public override Color GroupColor => TownOfUsColors.Jester;

    public ModdedEnumOption<RoleListOption> Slot1 { get; } =
        new("Slot 1", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot2 { get; } =
        new("Slot 2", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot3 { get; } =
        new("Slot 3", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot4 { get; } =
        new("Slot 4", RoleListOption.ImpCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot5 { get; } =
        new("Slot 5", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot6 { get; } =
        new("Slot 6", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot7 { get; } =
        new("Slot 7", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot8 { get; } =
        new("Slot 8", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot9 { get; } =
        new("Slot 9", RoleListOption.ImpCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot10 { get; } =
        new("Slot 10", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot11 { get; } =
        new("Slot 11", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot12 { get; } =
        new("Slot 12", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot13 { get; } =
        new("Slot 13", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot14 { get; } =
        new("Slot 14", RoleListOption.ImpCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot15 { get; } =
        new("Slot 15", RoleListOption.CrewCommon, RoleOptions.OptionStrings);
}