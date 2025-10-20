using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterSkeldOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Skeld";
    public override uint GroupPriority => 2;
    public override Color GroupColor => new Color32(188, 206, 200, 255);
    
    [ModdedNumberOption("Vision Multiplier", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float VisionMultiplier { get; set; } = 1f;

    [ModdedNumberOption("Increased Short Tasks", 0f, 5f)]
    public float IncreasedShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Increased Long Tasks", 0f, 3f)]
    public float IncreasedLongTasks { get; set; } = 0f;

    [ModdedToggleOption("Skeld Doors Are Polus Doors")]
    public bool SkeldPolusDoors { get; set; } = false;
    
    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("Oxygen Sabotage Countdown", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("Reactor Sabotage Countdown", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers
    };
}