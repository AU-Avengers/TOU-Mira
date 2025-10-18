using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TownOfUs.Options.Maps;

public sealed class BetterSkeldOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Skeld";
    public override uint GroupPriority => 3;

    [ModdedToggleOption("Skeld Doors Are Polus Doors")]
    public bool SkeldPolusDoors { get; set; } = false;
}