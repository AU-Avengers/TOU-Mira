using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterLevelImpostorOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.BetterLevelImpostor");
    public override uint GroupPriority => 9;
    public override Func<bool> GroupVisible => () => ModCompatibility.LILoaded;
    public override Color GroupColor => new Color32(16, 131, 176, 255);
    public override OptionNotifConfiguration Configuration => new(
        GroupColor,
        TmpSpriteUtils.CreateSpriteAsset(
            TouAssets.IconLevelImposter.LoadAsset(),
            "AmongUs.Map.LevelImposter",
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

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsNoLadderCooldown")]
    public bool NoLadderCooldown { get; set; } = true;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeOxygenSaboTimer")]
    public bool ChangeOxygenSaboTimer { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeReactorSaboTimer")]
    public bool ChangeReactorSaboTimer { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.BetterMaps.Option.MapsChangeMixUpSaboTimer")]
    public bool ChangeMixUpSaboTimer { get; set; } = false;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownOxygen", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.ChangeOxygenSaboTimer
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboCountdownReactor", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.ChangeReactorSaboTimer
    };

    public ModdedNumberOption SaboCountdownMixUp { get; set; } = new("TownOfUsMira.BetterMaps.Option.MapsSaboDurationMixUp", 10f, 5f, 60f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.ChangeMixUpSaboTimer
    };
}