using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterLevelImpostorOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Level Impostor";
    public override uint GroupPriority => 9;
    public override Func<bool> GroupVisible => () => ModCompatibility.LILoaded;
    public override Color GroupColor => new Color32(16, 131, 176, 255);

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
        new("TouOptionBetterMapsOffsetShortTasks", 0f, -5f, 5f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig) ==
                MapTweakMode.PerMap
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TouOptionBetterMapsOffsetLongTasks", 0f, -3f, 3f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GlobalBetterMapOptions.GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapLongTaskConfig) ==
                MapTweakMode.PerMap
        };

    /*[ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("Oxygen Sabotage Countdown", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSubmergedOptions>.Instance.ChangeSaboTimers
    };*/
}