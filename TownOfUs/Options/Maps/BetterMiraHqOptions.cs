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
    
    [ModdedNumberOption("Vision Multiplier", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float VisionMultiplier { get; set; } = 1f;

    [ModdedNumberOption("Cooldown Decrease", 0f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownDecrease { get; set; } = 0f;

    [ModdedNumberOption("Increased Short Tasks", 0f, 5f)]
    public float IncreasedShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Increased Long Tasks", 0f, 3f)]
    public float IncreasedLongTasks { get; set; } = 0f;

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("Oxygen Sabotage Countdown", 45f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterMiraHqOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("Reactor Sabotage Countdown", 45f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterMiraHqOptions>.Instance.ChangeSaboTimers
    };
}