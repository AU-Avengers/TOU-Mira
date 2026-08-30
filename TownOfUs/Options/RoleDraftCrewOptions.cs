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

    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconDraftMode.LoadAsset(),
            "TouMira.Gamemode.DraftMode",
            1.45f));

    public override string GroupName => MiraLocaleManager.Get("TouOptionTitleRoleDraftCrew");
    public override uint GroupPriority => 3;

    public ModdedNumberOption MaxCrewInvestigative { get; set; } =
        new("TouOptionRoleDraftCrewMaxInvestigative", 5f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewKilling { get; set; } =
        new("TouOptionRoleDraftCrewMaxKilling", 3f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewPower { get; set; } =
        new("TouOptionRoleDraftCrewMaxPower", 2f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewProtective { get; set; } =
        new("TouOptionRoleDraftCrewMaxProtective", 2f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxCrewSupport { get; set; } =
        new("TouOptionRoleDraftCrewMaxSupport", 3f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");
}