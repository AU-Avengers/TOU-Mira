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
    public override uint GroupPriority => 6;
    public override Color GroupColor => new Color32(255, 76, 73, 255);

    [ModdedNumberOption("TouOptionBetterMapsSpeedMultiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float SpeedMultiplier { get; set; } = 1f;

    [ModdedNumberOption("TouOptionBetterMapsCrewVisionMultiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float CrewVisionMultiplier { get; set; } = 1f;
    
    [ModdedNumberOption("TouOptionBetterMapsImpVisionMultiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float ImpVisionMultiplier { get; set; } = 1f;

    [ModdedNumberOption("TouOptionBetterMapsCooldownOffset", -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownOffset { get; set; } = 0f;

    [ModdedNumberOption("TouOptionBetterMapsOffsetShortTasks", -5f, 5f)]
    public float OffsetShortTasks { get; set; } = 0f;

    [ModdedNumberOption("TouOptionBetterMapsOffsetLongTasks", -3f, 3f)]
    public float OffsetLongTasks { get; set; } = 0f;

    public ModdedEnumOption AirshipDoorType { get; set; } = new("TouOptionBetterAirshipDoorType",
        (int)MapDoorType.Airship, typeof(MapDoorType),
        [
            "TouOptionBetterDoorsEnumSkeld", "TouOptionBetterDoorsEnumPolus", "TouOptionBetterDoorsEnumAirship",
            "TouOptionBetterDoorsEnumFungle", "TouOptionBetterDoorsEnumSubmerged", "TouOptionBetterDoorsEnumNoDoors",
            "TouOptionBetterDoorsEnumRandom"
        ]);

    [ModdedEnumOption("TouOptionBetterAirshipSpawnMode", typeof(SpawnModes), ["TouOptionBetterAirshipSpawnEnumNormal", "TouOptionBetterAirshipSpawnEnumSameSpawns", "TouOptionBetterAirshipSpawnEnumHostChoosesOne"])]
    public SpawnModes SpawnMode { get; set; } = SpawnModes.Normal;

    public ModdedEnumOption SingleLocation { get; } = new ModdedEnumOption("TouOptionBetterAirshipSingleLocation", 0, typeof(Locations),
        ["TouOptionBetterAirshipSpawnLocationEnumMainHall", "TouOptionBetterAirshipSpawnLocationEnumKitchen", "TouOptionBetterAirshipSpawnLocationEnumCargoBay", "TouOptionBetterAirshipSpawnLocationEnumEngineRoom", "TouOptionBetterAirshipSpawnLocationEnumBrig", "TouOptionBetterAirshipSpawnLocationEnumRecords"])
    {
        Visible = () => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpawnMode == SpawnModes.HostChoosesOne,
    };

    [ModdedToggleOption("TouOptionBetterMapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TouOptionBetterMapsSaboCountdownCrashCourse", 90f, 15f, 90f,
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