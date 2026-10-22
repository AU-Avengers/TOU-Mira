using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterSkeldOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.BetterSkeld");
    public override uint GroupPriority => 3;
    public override Color GroupColor => new Color32(188, 206, 200, 255);
    public override bool ShowInMatchInfoGuide => true;
    public override LoadableAsset<Sprite> Sprite => TouAssets.IconSkeld;
    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconSkeld,
            "AmongUs.Map.Skeld",
            1.45f));

    public ModdedToggleOption CamoComms { get; set; } =
        new("TownOfUsMira.AdvancedSabo.Option.CamouflageComms", true)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCamoCommsConfig == MapTweakMode.PerMap
        };

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsSpeedMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapSpeedConfig == MapTweakMode.PerMap
        };

    public ModdedNumberOption CrewVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCrewVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCrewVisionConfig == MapTweakMode.PerMap
        };

    public ModdedNumberOption ImpVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsImpVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapImpVisionConfig == MapTweakMode.PerMap
        };

    public ModdedNumberOption CooldownOffset { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCooldownOffset", 0f, -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCooldownConfig == MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetShortTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetShortTasks", 0f, -5f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig == MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetLongTasks", 0f, -3f, 3f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapLongTaskConfig == MapTweakMode.PerMap
        };

    public ModdedEnumOption<MapDoorType> SkeldDoorType { get; set; } = new("TownOfUsMira.BetterMaps.Option.SkeldDoorType", MapDoorType.Skeld,
    [
        "TownOfUsMira.BetterMaps.Option.DoorsEnumSkeld", "TownOfUsMira.BetterMaps.Option.DoorsEnumPolus", "TownOfUsMira.BetterMaps.Option.DoorsEnumAirship",
        "TownOfUsMira.BetterMaps.Option.DoorsEnumFungle", "TownOfUsMira.BetterMaps.Option.DoorsEnumSubmerged", "TownOfUsMira.BetterMaps.Option.DoorsEnumNoDoors",
        "TownOfUsMira.BetterMaps.Option.DoorsEnumRandom"
    ]);

    public ModdedEnumOption<SkeldVentMode> BetterVentNetwork { get; set; } = new("TownOfUsMira.BetterMaps.Option.SkeldVentNetwork",
        SkeldVentMode.Normal,
        [
            "TownOfUsMira.BetterMaps.Option.SkeldVentModeEnumNormal", "TownOfUsMira.BetterMaps.Option.SkeldVentModeEnumFourGroups"
        ]);

    public ModdedEnumOption<SkeldTheme> MapTheme { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsTheme",
        SkeldTheme.Auto,
        [
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumAuto", "TownOfUsMira.BetterMaps.Option.MapsThemeEnumBasic",
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumBirthday", "TownOfUsMira.BetterMaps.Option.MapsThemeEnumHalloween",
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumTvShow"
        ]);

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownOxygen", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownReactor", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers
    };

    public static float MSaboCountdownReactor => OptionGroupSingleton<BetterSkeldOptions>.Instance.SaboCountdownReactor.Value;
    public static bool MChangeSaboTimers => OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers;
}

public enum SkeldVentMode
{
    Normal,
    FourGroups,
}

public enum SkeldTheme
{
    Auto,
    Basic,
    Birthday,
    Halloween,
    TvShow,
}