using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class HnsImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("HnsOptionTitleSeekerModifiers");
    // public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.HideAndSeek;
    public override Func<bool> GroupVisible => () => false;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 1;

    public ModdedNumberOption AdministratorChance { get; } =
        new("TownOfUsMira.HideAndSeek.Role.Option.AdministratorChanceNA", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption DisperserChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.DisperserChanceNA", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent);
}