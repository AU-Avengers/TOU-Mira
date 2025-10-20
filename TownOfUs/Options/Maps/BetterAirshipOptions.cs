using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterAirshipOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Airship";
    public override uint GroupPriority => 5;
    public override Color GroupColor => new Color32(255, 76, 73, 255);

    [ModdedNumberOption("Cooldown Increase", 0f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownIncrease { get; set; } = 0f;

    [ModdedNumberOption("Decreased Short Tasks", 0f, 5f)]
    public float DecreasedShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Decreased Long Tasks", 0f, 3f)]
    public float DecreasedLongTasks { get; set; } = 0f;

    [ModdedToggleOption("Airship Doors Are Polus Doors")]
    public bool AirshipPolusDoors { get; set; } = false;

    [ModdedEnumOption("Spawn Mode", typeof(SpawnModes), ["Normal", "Everyone Has Same Spawns", "Host Chooses One"])]
    public SpawnModes SpawnMode { get; set; } = SpawnModes.Normal;

    public ModdedEnumOption SingleLocation { get; } = new ModdedEnumOption("Spawn At", 0, typeof(Locations),
        ["Main Hall", "Kitchen", "Cargo Bay", "Engine Room", "Brig", "Records"])
    {
        Visible = () => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpawnMode == SpawnModes.HostChoosesOne,
    };

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("Crash Course Sabotage Countdown", 90f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterAirshipOptions>.Instance.ChangeSaboTimers
    };

    public enum SpawnModes
    {
        Normal,
        SameSpawns,
        HostChoosesOne
    }

    public enum Locations
    {
        MainHall,
        Kitchen,
        CargoBay,
        EngineRoom,
        Brig,
        Records,
    }
}