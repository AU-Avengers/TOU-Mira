using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class HnsImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Seeker Modifiers";
    // public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.HideAndSeek;
    public override Func<bool> GroupVisible => () => false;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 1;

    public ModdedNumberOption AdministratorChance { get; } =
        new("Administrator Chance (N/A)", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption DisperserChance { get; } =
        new("Disperser Chance (N/A)", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent);
}