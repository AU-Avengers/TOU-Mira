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

    public ModdedToggleOption CamoComms { get; set; } =
        new("TouOptionAdvancedSaboCamouflageComms", true)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCamoCommsConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TouOptionBetterMapsSpeedMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapSpeedConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption CrewVisionMultiplier { get; set; } =
        new("TouOptionBetterMapsCrewVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCrewVisionConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption ImpVisionMultiplier { get; set; } =
        new("TouOptionBetterMapsImpVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapImpVisionConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption CooldownOffset { get; set; } =
        new("TouOptionBetterMapsCooldownOffset", 0f, -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCooldownConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetShortTasks { get; set; } =
        new("TouOptionBetterMapsOffsetShortTasks", 0f, -5f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TouOptionBetterMapsOffsetLongTasks", 0f, -3f, 3f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapLongTaskConfig) ==
                MapTweakMode.PerMap
        };

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

    [ModdedToggleOption("TouOptionBetterMapsNoLadderCooldown")]
    public bool NoLadderCooldown { get; set; } = true;

    /*public ModdedEnumOption MapTheme { get; set; } = new("TouOptionBetterMapsTheme",
        (int)PolusTheme.Auto, typeof(PolusTheme),
        [
            "TouOptionBetterMapsThemeEnumAuto", "TouOptionBetterMapsThemeEnumBasic",
            "TouOptionBetterMapsThemeEnumHalloween"
        ]);*/

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