using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterFungleOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Fungle";
    public override uint GroupPriority => 7;
    public override Color GroupColor => new Color32(239, 98, 162, 255);

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


    public ModdedEnumOption FungleDoorType { get; set; } = new("Door Type on Fungle", (int)MapDoorType.Fungle, typeof(MapDoorType));

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("Reactor Sabotage Countdown", 60f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterFungleOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownMixUp { get; set; } = new("Mix-Up Sabotage Duration", 10f, 5f, 60f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterFungleOptions>.Instance.ChangeSaboTimers
    };
}