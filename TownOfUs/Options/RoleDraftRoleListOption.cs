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

    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.RoleDraftRoleList");
    public override uint GroupPriority => 3;
    public override Color GroupColor => TownOfUsColors.Jester;

    public ModdedOptionList<ModdedEnumOption<RoleListOption>> Slots { get; } =
        new(15, i => new($"TouOptionRoleDraftRoleListSlot{i + 1}",
                         i + 2 % 5 == 0 ? RoleListOption.ImpCommon : RoleListOption.CrewCommon,
                         RoleOptions.OptionStrings));
}