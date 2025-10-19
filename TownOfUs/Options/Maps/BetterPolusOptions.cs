using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class BetterPolusOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Polus";
    public override uint GroupPriority => 4;

    [ModdedToggleOption("Better Polus Vent Network")]
    public bool BPVentNetwork { get; set; } = false;

    [ModdedToggleOption("Vitals Moved To Lab")]
    public bool BPVitalsInLab { get; set; } = false;

    [ModdedToggleOption("Cold Temp Moved To Death Valley")]
    public bool BPTempInDeathValley { get; set; } = false;

    [ModdedToggleOption("Reboot Wifi And Chart Course Swapped")]
    public bool BPSwapWifiAndChart { get; set; } = false;

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("Seismic Stabilizer Sabotage Countdown", 60f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterPolusOptions>.Instance.ChangeSaboTimers
    };
}