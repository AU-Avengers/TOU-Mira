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
    public override uint GroupPriority => 7;
    public override Func<bool> GroupVisible => () => ModCompatibility.SubLoaded;
    public override Color GroupColor => new Color32(50, 100, 255, 255);

    [ModdedNumberOption("Cooldown Increase", 0f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownIncrease { get; set; } = 0f;

    [ModdedNumberOption("Decreased Short Tasks", 0f, 5f)]
    public float DecreasedShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Decreased Long Tasks", 0f, 3f)]
    public float DecreasedLongTasks { get; set; } = 0f;

    [ModdedToggleOption("Submerged Doors Are Polus Doors")]
    public bool SubmergedPolusDoors { get; set; } = false;

    [ModdedEnumOption("Spawn Mode", typeof(SubSpawnLocation), ["Selectable", "Upper Deck", "Lower Deck"])]
    public SubSpawnLocation SpawnMode { get; set; } = SubSpawnLocation.Selectable;

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("Oxygen Sabotage Countdown", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSubmergedOptions>.Instance.ChangeSaboTimers
    };

    public enum SubSpawnLocation
    {
        Selectable,
        UpperDeck,
        LowerDeck
    }
}