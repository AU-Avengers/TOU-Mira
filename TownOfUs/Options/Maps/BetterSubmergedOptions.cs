using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterSubmergedOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Submerged";
    public override uint GroupPriority => 8;
    public override Func<bool> GroupVisible => () => ModCompatibility.SubLoaded;
    public override Color GroupColor => new Color32(10, 150, 255, 255);

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

    public ModdedEnumOption SubmergedDoorType { get; set; } = new("Door Type on Submerged", (int)MapDoorType.Submerged, typeof(MapDoorType));

    /*
    [ModdedEnumOption("Spawn Mode", typeof(SubSpawnLocation), ["Selectable", "Upper Deck", "Lower Deck"])]
    public SubSpawnLocation SpawnMode { get; set; } = SubSpawnLocation.Selectable;

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("Oxygen Sabotage Countdown", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSubmergedOptions>.Instance.ChangeSaboTimers
    };*/

    public enum SubSpawnLocation
    {
        Selectable,
        UpperDeck,
        LowerDeck
    }
}