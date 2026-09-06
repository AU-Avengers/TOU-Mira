using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterAirshipOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.BetterAirship");
    public override uint GroupPriority => 6;
    public override Color GroupColor => new Color32(255, 76, 73, 255);

    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconAirship.LoadAsset(),
            "AmongUs.Map.Airship",
            1.45f));

    public ModdedToggleOption CamoComms { get; set; } =
        new("TownOfUsMira.AdvancedSabo.Option.CamouflageComms", true)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCamoCommsConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsSpeedMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapSpeedConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption CrewVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCrewVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCrewVisionConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption ImpVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsImpVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapImpVisionConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption CooldownOffset { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCooldownOffset", 0f, -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCooldownConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetShortTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetShortTasks", 0f, -5f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetLongTasks", 0f, -3f, 3f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapLongTaskConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedEnumOption AirshipDoorType { get; set; } = new("TownOfUsMira.BetterMaps.Option.AirshipDoorType",
        (int)MapDoorType.Airship, typeof(MapDoorType),
        [
            "TownOfUsMira.BetterMaps.Option.DoorsEnumSkeld", "TownOfUsMira.BetterMaps.Option.DoorsEnumPolus", "TownOfUsMira.BetterMaps.Option.DoorsEnumAirship",
            "TownOfUsMira.BetterMaps.Option.DoorsEnumFungle", "TownOfUsMira.BetterMaps.Option.DoorsEnumSubmerged", "TownOfUsMira.BetterMaps.Option.DoorsEnumNoDoors",
            "TownOfUsMira.BetterMaps.Option.DoorsEnumRandom"
        ]);

    [ModdedEnumOption("TownOfUsMira.BetterMaps.Option.AirshipSpawnMode", typeof(SpawnModes), ["TownOfUsMira.BetterMaps.Option.AirshipSpawnEnumNormal", "TownOfUsMira.BetterMaps.Option.AirshipSpawnEnumSameSpawns", "TownOfUsMira.BetterMaps.Option.AirshipSpawnEnumHostChoosesOne"])]
    public SpawnModes SpawnMode { get; set; } = SpawnModes.Normal;

    public ModdedEnumOption SingleLocation { get; } = new ModdedEnumOption("TownOfUsMira.BetterMaps.Option.AirshipSingleLocation", 0, typeof(Locations),
        ["TownOfUsMira.BetterMaps.Option.AirshipSpawnLocationEnumMainHall", "TownOfUsMira.BetterMaps.Option.AirshipSpawnLocationEnumKitchen", "TownOfUsMira.BetterMaps.Option.AirshipSpawnLocationEnumCargoBay", "TownOfUsMira.BetterMaps.Option.AirshipSpawnLocationEnumEngineRoom", "TownOfUsMira.BetterMaps.Option.AirshipSpawnLocationEnumBrig", "TownOfUsMira.BetterMaps.Option.AirshipSpawnLocationEnumRecords"])
    {
        Visible = () => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpawnMode == SpawnModes.HostChoosesOne,
    };

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsNoLadderCooldown")]
    public bool NoLadderCooldown { get; set; } = true;

    /*public ModdedEnumOption MapTheme { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsTheme",
        (int)PolusTheme.Auto, typeof(PolusTheme),
        [
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumAuto", "TownOfUsMira.BetterMaps.Option.MapsThemeEnumBasic",
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumHalloween"
        ]);*/

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownCrashCourse", 90f, 15f, 90f,
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