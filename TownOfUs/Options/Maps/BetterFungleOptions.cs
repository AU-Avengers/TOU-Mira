using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class BetterFungleOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Fungle";
    public override uint GroupPriority => 6;
    [ModdedNumberOption("Mix-Up Sabotage Duration", 5f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float SaboCountdownMixUp { get; set; } = 10f;

    [ModdedNumberOption("Reactor Sabotage Countdown", 15f, 90f, 5f, MiraNumberSuffixes.Seconds)]
    public float SaboCountdownReactor { get; set; } = 60f;
}