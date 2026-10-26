using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class GlobalBetterMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.GlobalBetterMaps");
    public override uint GroupPriority => 0;
    public override bool ShowInMatchInfoGuide => true;
    public override string MatchInfoGuidePageKey => "TownOfUsMira.OptionGroups.BetterMaps";
    public override string MatchInfoGuidePageTitle => MiraLocaleManager.Get("WikiSettingsBetterMapsSettingsTitle");
    public override LoadableAsset<Sprite> Sprite => TouAssets.AdminSprite;

    public static readonly string[] GlobalOpts =
    [
        "TownOfUsMira.GlobalBetterMap.Option.ChangeEnumOff", "TownOfUsMira.GlobalBetterMap.Option.ChangeEnumOn",
        "TownOfUsMira.GlobalBetterMap.Option.ChangeEnumPerMap"
    ];

    public ModdedEnumOption<MapTweakMode> GlobalMapCamoCommsConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.CamouflageComms",
        MapTweakMode.GlobalOff, GlobalOpts);

    public ModdedEnumOption<MapTweakMode> GlobalMapSpeedConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.SpeedMultiplier",
        MapTweakMode.PerMap, GlobalOpts);

    public ModdedEnumOption<MapTweakMode> GlobalMapCrewVisionConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.CrewVisionMultiplier",
        MapTweakMode.PerMap, GlobalOpts);

    public ModdedEnumOption<MapTweakMode> GlobalMapImpVisionConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.ImpVisionMultiplier",
        MapTweakMode.PerMap, GlobalOpts);

    public ModdedEnumOption<MapTweakMode> GlobalMapCooldownConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.CooldownOffset",
        MapTweakMode.PerMap, GlobalOpts);

    public ModdedEnumOption<MapTweakMode> GlobalMapShortTaskConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.OffsetShortTasks",
        MapTweakMode.PerMap, GlobalOpts);

    public ModdedEnumOption<MapTweakMode> GlobalMapLongTaskConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.OffsetLongTasks",
        MapTweakMode.PerMap, GlobalOpts);

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsSpeedMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapSpeedConfig == MapTweakMode.GlobalOn
        };

    public ModdedNumberOption CrewVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCrewVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCrewVisionConfig == MapTweakMode.GlobalOn
        };

    public ModdedNumberOption ImpVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsImpVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapImpVisionConfig == MapTweakMode.GlobalOn
        };

    public ModdedNumberOption CooldownOffset { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCooldownOffset", 0f, -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCooldownConfig == MapTweakMode.GlobalOn
        };

    public ModdedNumberOption OffsetShortTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetShortTasks", 0f, -5f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig == MapTweakMode.GlobalOn
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetLongTasks", 0f, -3f, 3f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapLongTaskConfig == MapTweakMode.GlobalOn
        };
}

public enum MapTweakMode
{
    GlobalOff,
    GlobalOn,
    PerMap
}