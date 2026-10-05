using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterMiraHqOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.BetterMira");
    public override uint GroupPriority => 4;
    public override Color GroupColor => new Color32(255, 128, 100, 255);
    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconMira.LoadAsset(),
            "AmongUs.Map.Mira",
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

    public ModdedEnumOption BetterVentNetwork { get; set; } = new("TownOfUsMira.BetterMaps.Option.MiraHqVentNetwork",
        (int)MiraVentMode.Normal, typeof(MiraVentMode),
        [
            "TownOfUsMira.BetterMaps.Option.MiraHqVentModeEnumNormal", "TownOfUsMira.BetterMaps.Option.MiraHqVentModeEnumThreeGroups", "TownOfUsMira.BetterMaps.Option.MiraHqVentModeEnumFourGroups"
        ]);

    public ModdedEnumOption MapTheme { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsTheme",
        (int)PolusTheme.Auto, typeof(PolusTheme),
        [
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumAuto", "TownOfUsMira.BetterMaps.Option.MapsThemeEnumBasic",
            "TownOfUsMira.BetterMaps.Option.MapsThemeEnumHalloween"
        ]);

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownOxygen", 45f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterMiraHqOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownReactor", 45f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterMiraHqOptions>.Instance.ChangeSaboTimers
    };
}

public enum MiraVentMode
{
    Normal,
    ThreeGroups,
    FourGroups
}