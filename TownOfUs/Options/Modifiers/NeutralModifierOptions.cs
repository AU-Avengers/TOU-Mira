using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Neutral Modifiers";
    public override Color GroupColor => TownOfUsColors.Neutral;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 4;

    [ModdedNumberOption("Double Shot Amount", 0, 5)]
    public float DoubleShotAmount { get; set; } = 0;

    public ModdedNumberOption DoubleShotChance { get; } =
        new("Double Shot Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotAmount > 0
        };
}