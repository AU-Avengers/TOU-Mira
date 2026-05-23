using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class RoleDraftImpOptions : AbstractOptionGroup
{
    private static bool HasImps => (int)OptionGroupSingleton<RoleDraftImpOptions>.Instance.MaxImpostors.Value > 0;

    public override Func<bool> GroupVisible => () =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft &&
        !OptionGroupSingleton<RoleOptions>.Instance.UseRoleListForPool;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;

    public override string GroupName => "Impostor Settings";
    public override uint GroupPriority => 2;

    public ModdedNumberOption MaxImpostors { get; set; } =
        new("Max Impostors Total", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxImpConcealing { get; set; } = new("Max Concealing Roles", 2f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasImps
    };

    public ModdedNumberOption MaxImpKilling { get; set; } = new("Max Killing Roles", 2f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasImps
    };

    public ModdedNumberOption MaxImpPower { get; set; } = new("Max Power Roles", 2f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasImps
    };

    public ModdedNumberOption MaxImpSupport { get; set; } = new("Max Support Roles", 2f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasImps
    };
}