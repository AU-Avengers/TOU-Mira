using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class BetterMiraHqOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Mira HQ";
    public override uint GroupPriority => 3;
    [ModdedNumberOption("Oxygen Sabotage Countdown", 15f, 90f, 5f, MiraNumberSuffixes.Seconds)]
    public float SaboCountdownOxygen { get; set; } = 45f;

    [ModdedNumberOption("Reactor Sabotage Countdown", 15f, 90f, 5f, MiraNumberSuffixes.Seconds)]
    public float SaboCountdownReactor { get; set; } = 45f;
}