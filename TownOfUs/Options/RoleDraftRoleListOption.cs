using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class RoleDraftRoleListOptions : AbstractOptionGroup
{
    public override Func<bool> GroupVisible => () =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft &&
        OptionGroupSingleton<RoleOptions>.Instance.UseRoleListForPool;

    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconDraftMode.LoadAsset(),
            "TouMira.Gamemode.DraftMode",
            1.45f));

    public override string GroupName => MiraLocaleManager.Get("TouOptionTitleRoleDraftRoleList");
    public override uint GroupPriority => 3;
    public override Color GroupColor => TownOfUsColors.Jester;

    public ModdedEnumOption<RoleListOption> Slot1 { get; } =
        new("TouOptionRoleDraftRoleListSlot1", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot2 { get; } =
        new("TouOptionRoleDraftRoleListSlot2", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot3 { get; } =
        new("TouOptionRoleDraftRoleListSlot3", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot4 { get; } =
        new("TouOptionRoleDraftRoleListSlot4", RoleListOption.ImpCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot5 { get; } =
        new("TouOptionRoleDraftRoleListSlot5", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot6 { get; } =
        new("TouOptionRoleDraftRoleListSlot6", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot7 { get; } =
        new("TouOptionRoleDraftRoleListSlot7", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot8 { get; } =
        new("TouOptionRoleDraftRoleListSlot8", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot9 { get; } =
        new("TouOptionRoleDraftRoleListSlot9", RoleListOption.ImpCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot10 { get; } =
        new("TouOptionRoleDraftRoleListSlot10", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot11 { get; } =
        new("TouOptionRoleDraftRoleListSlot11", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot12 { get; } =
        new("TouOptionRoleDraftRoleListSlot12", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot13 { get; } =
        new("TouOptionRoleDraftRoleListSlot13", RoleListOption.CrewCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot14 { get; } =
        new("TouOptionRoleDraftRoleListSlot14", RoleListOption.ImpCommon, RoleOptions.OptionStrings);

    public ModdedEnumOption<RoleListOption> Slot15 { get; } =
        new("TouOptionRoleDraftRoleListSlot15", RoleListOption.CrewCommon, RoleOptions.OptionStrings);
}