using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class ImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Impostor Modifiers";
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 3;

    [ModdedNumberOption("Circumvent Amount", 0, 5)]
    public float CircumventAmount { get; set; } = 0;

    public ModdedNumberOption CircumventChance { get; } =
        new("Circumvent Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventAmount > 0
        };

    [ModdedNumberOption("Deadly Quota Amount", 0, 5)]
    public float DeadlyQuotaAmount { get; set; } = 0;

    public ModdedNumberOption DeadlyQuotaChance { get; } =
        new("Deadly Quota Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaAmount > 0
        };

    [ModdedNumberOption("Disperser Amount", 0, 5)]
    public float DisperserAmount { get; set; } = 0;

    public ModdedNumberOption DisperserChance { get; } =
        new("Disperser Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.DisperserAmount > 0
        };

    [ModdedNumberOption("Double Shot Amount", 0, 5)]
    public float DoubleShotAmount { get; set; } = 0;

    public ModdedNumberOption DoubleShotChance { get; } =
        new("Double Shot Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotAmount > 0
        };

    [ModdedNumberOption("Saboteur Amount", 0, 5)]
    public float SaboteurAmount { get; set; } = 0;

    public ModdedNumberOption SaboteurChance { get; } =
        new("Saboteur Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurAmount > 0
        };

    [ModdedNumberOption("Telepath Amount", 0, 5)]
    public float TelepathAmount { get; set; } = 0;

    public ModdedNumberOption TelepathChance { get; } =
        new("Telepath Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.TelepathAmount > 0
        };

    [ModdedNumberOption("Underdog Amount", 0, 5)]
    public float UnderdogAmount { get; set; } = 0;

    public ModdedNumberOption UnderdogChance { get; } =
        new("Underdog Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.UnderdogAmount > 0
        };
}