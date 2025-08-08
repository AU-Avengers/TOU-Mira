using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Modifiers;

public sealed class UniversalModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 1;

    [ModdedNumberOption("Button Barry Amount", 0, 1)]
    public float ButtonBarryAmount { get; set; } = 0;

    public ModdedNumberOption ButtonBarryChance { get; } =
        new("Button Barry Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ButtonBarryAmount > 0
        };

    [ModdedNumberOption("Drunk Amount", 0, 5)]
    public float DrunkAmount { get; set; } = 0;

    public ModdedNumberOption DrunkChance { get; } = new("Drunk Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkAmount > 0
    };

    [ModdedNumberOption("Flash Amount", 0, 5)]
    public float FlashAmount { get; set; } = 0;

    public ModdedNumberOption FlashChance { get; } = new("Flash Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashAmount > 0
    };

    [ModdedNumberOption("Giant Amount", 0, 5)]
    public float GiantAmount { get; set; } = 0;

    public ModdedNumberOption GiantChance { get; } = new("Giant Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantAmount > 0
    };

    [ModdedNumberOption("Immovable Amount", 0, 5)]
    public float ImmovableAmount { get; set; } = 0;

    public ModdedNumberOption ImmovableChance { get; } =
        new("Immovable Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ImmovableAmount > 0
        };

    [ModdedNumberOption("Mini Amount", 0, 5)]
    public float MiniAmount { get; set; } = 0;

    public ModdedNumberOption MiniChance { get; } = new("Mini Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniAmount > 0
    };

    [ModdedNumberOption("Radar Amount", 0, 5)]
    public float RadarAmount { get; set; } = 0;

    public ModdedNumberOption RadarChance { get; } = new("Radar Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.RadarAmount > 0
    };

    [ModdedNumberOption("Satellite Amount", 0, 5)]
    public float SatelliteAmount { get; set; } = 0;

    public ModdedNumberOption SatelliteChance { get; } =
        new("Satellite Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteAmount > 0
        };

    [ModdedNumberOption("Shy Amount", 0, 5)]
    public float ShyAmount { get; set; } = 0;

    public ModdedNumberOption ShyChance { get; } = new("Shy Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ShyAmount > 0
    };

    [ModdedNumberOption("Sixth Sense Amount", 0, 5)]
    public float SixthSenseAmount { get; set; } = 0;

    public ModdedNumberOption SixthSenseChance { get; } =
        new("Sixth Sense Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.SixthSenseAmount > 0
        };

    [ModdedNumberOption("Sleuth Amount", 0, 5)]
    public float SleuthAmount { get; set; } = 0;

    public ModdedNumberOption SleuthChance { get; } =
        new("Sleuth Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthAmount > 0
        };

    [ModdedNumberOption("Tiebreaker Amount", 0, 1)]
    public float TiebreakerAmount { get; set; } = 0;

    public ModdedNumberOption TiebreakerChance { get; } =
        new("Tiebreaker Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerAmount > 0
        };
}