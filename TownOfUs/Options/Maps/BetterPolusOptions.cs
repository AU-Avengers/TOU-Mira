using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace TownOfUs.Options.Maps;

public sealed class BetterPolusOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Polus";
    public override uint GroupPriority => 5;

    [ModdedToggleOption("Better Polus Vent Network")]
    public bool BPVentNetwork { get; set; } = false;

    [ModdedToggleOption("Polus: Vitals Moved To Lab")]
    public bool BPVitalsInLab { get; set; } = false;

    [ModdedToggleOption("Polus: Cold Temp Moved To Death Valley")]
    public bool BPTempInDeathValley { get; set; } = false;

    [ModdedToggleOption("Polus: Reboot Wifi And Chart Course Swapped")]
    public bool BPSwapWifiAndChart { get; set; } = false;
}