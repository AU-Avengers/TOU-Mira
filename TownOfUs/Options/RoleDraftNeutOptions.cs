using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class RoleDraftNeutOptions : AbstractOptionGroup
{
    private static bool HasNeuts => (int)OptionGroupSingleton<RoleDraftNeutOptions>.Instance.MaxNeutrals.Value > 0;

    public override Func<bool> GroupVisible => () =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft &&
        !OptionGroupSingleton<RoleOptions>.Instance.UseRoleListForPool;
    public override Color GroupColor => TownOfUsColors.Neutral;

    public override string GroupName => "Neutral Settings";
    public override uint GroupPriority => 3;

    public ModdedNumberOption MaxNeutrals { get; set; } = new("Max Neutral Roles", 3f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxNeutBenign { get; set; } = new("Max Benign Roles", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasNeuts
    };

    public ModdedNumberOption MaxNeutEvil { get; set; } = new("Max Evil Roles", 1f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasNeuts
    };

    public ModdedNumberOption MaxNeutKilling { get; set; } = new("Max Killing Roles", 1f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasNeuts
    };

    public ModdedNumberOption MaxNeutOutlier { get; set; } = new("Max Outlier Roles", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => HasNeuts
    };
}