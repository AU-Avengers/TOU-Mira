using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterPolusOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.BetterPolus");
    public override uint GroupPriority => 5;
    public override Color GroupColor => new Color32(157, 146, 198, 255);
    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconPolus.LoadAsset(),
            "AmongUs.Map.Polus",
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

    public ModdedEnumOption PolusDoorType { get; set; } = new("TownOfUsMira.BetterMaps.Option.PolusDoorType", (int)MapDoorType.Polus, typeof(MapDoorType),
    [
        "TownOfUsMira.BetterMaps.Option.DoorsEnumSkeld", "TownOfUsMira.BetterMaps.Option.DoorsEnumPolus", "TownOfUsMira.BetterMaps.Option.DoorsEnumAirship",
        "TownOfUsMira.BetterMaps.Option.DoorsEnumFungle", "TownOfUsMira.BetterMaps.Option.DoorsEnumSubmerged", "TownOfUsMira.BetterMaps.Option.DoorsEnumNoDoors",
        "TownOfUsMira.BetterMaps.Option.DoorsEnumRandom"
    ]);

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.PolusVentNetwork")]
    public bool BPVentNetwork { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.PolusVitalsInLab")]
    public bool BPVitalsInLab { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.PolusTempInDeathValley")]
    public bool BPTempInDeathValley { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.PolusSwapWikiAndChart")]
    public bool BPSwapWifiAndChart { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.PolusMoveToiletVent")]
    public bool MoveToiletVent { get; set; } = false;

    public ModdedEnumOption MapTheme { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsTheme",
        (int)PolusTheme.Auto, typeof(PolusTheme),
        [
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumAuto", "TownOfUsMira.BetterMaps.Option.MapsThemeEnumBasic",
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumHalloween"
        ]);

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownSeismicStabilizer", 60f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterPolusOptions>.Instance.ChangeSaboTimers
    };
}

public enum PolusTheme
{
    Auto,
    Basic,
    Halloween,
}