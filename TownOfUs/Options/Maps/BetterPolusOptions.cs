using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterPolusOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Polus";
    public override uint GroupPriority => 5;
    public override Color GroupColor => new Color32(157, 146, 198, 255);

    [ModdedNumberOption("Speed Multiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float SpeedMultiplier { get; set; } = 1f;

    [ModdedNumberOption("Crew Vision Multiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float CrewVisionMultiplier { get; set; } = 1f;
    
    [ModdedNumberOption("Impostor Vision Multiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float ImpVisionMultiplier { get; set; } = 1f;

    [ModdedNumberOption("Cooldown Offset", -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownOffset { get; set; } = 0f;

    [ModdedNumberOption("Offset Short Tasks", -5f, 5f)]
    public float OffsetShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Offset Long Tasks", -3f, 3f)]
    public float OffsetLongTasks { get; set; } = 0f;

    public ModdedEnumOption PolusDoorType { get; set; } = new("Door Type on Polus", (int)MapDoorType.Polus, typeof(MapDoorType));

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