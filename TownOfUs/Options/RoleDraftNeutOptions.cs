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

    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconDraftMode.LoadAsset(),
            "TouMira.Gamemode.DraftMode",
            1.45f));

    public override string GroupName => MiraLocaleManager.Get("TouOptionTitleRoleDraftNeut");
    public override uint GroupPriority => 3;

    public ModdedNumberOption MaxNeutrals { get; set; } =
        new("TouOptionRoleDraftNeutMaxNeutrals", 3f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxNeutBenign { get; set; } =
        new("TouOptionRoleDraftNeutMaxBenign", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => HasNeuts
        };

    public ModdedNumberOption MaxNeutEvil { get; set; } =
        new("TouOptionRoleDraftNeutMaxEvil", 1f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => HasNeuts
        };

    public ModdedNumberOption MaxNeutKilling { get; set; } =
        new("TouOptionRoleDraftNeutMaxKilling", 1f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => HasNeuts
        };

    public ModdedNumberOption MaxNeutOutlier { get; set; } =
        new("TouOptionRoleDraftNeutMaxOutlier", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => HasNeuts
        };
}