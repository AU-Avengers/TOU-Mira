using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class GlobalBetterMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Global Better Maps";
    public override uint GroupPriority => 0;
    public static MapTweakMode GetMapTweakMode(ModdedEnumOption option) => (MapTweakMode)option.Value;

    public ModdedEnumOption GlobalMapCamoCommsConfig { get; set; } = new("TouOptionGlobalBetterMapCamouflageComms",
        (int)MapTweakMode.GlobalOff, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedEnumOption GlobalMapSpeedConfig { get; set; } = new("TouOptionGlobalBetterMapSpeedMultiplier",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedEnumOption GlobalMapCrewVisionConfig { get; set; } = new(
        "TouOptionGlobalBetterMapCrewVisionMultiplier", (int)MapTweakMode.GlobalOff, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedEnumOption GlobalMapImpVisionConfig { get; set; } = new("TouOptionGlobalBetterMapImpVisionMultiplier",
        (int)MapTweakMode.GlobalOff, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedEnumOption GlobalMapCooldownConfig { get; set; } = new("TouOptionGlobalBetterMapCooldownOffset",
        (int)MapTweakMode.GlobalOff, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedEnumOption GlobalMapShortTaskConfig { get; set; } = new("TouOptionGlobalBetterMapOffsetShortTasks",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedEnumOption GlobalMapLongTaskConfig { get; set; } = new("TouOptionGlobalBetterMapOffsetLongTasks",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode),
        [
            "TouOptionGlobalBetterMapChangeEnumOff", "TouOptionGlobalBetterMapChangeEnumOn",
            "TouOptionGlobalBetterMapChangeEnumPerMap",
        ]);

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TouOptionBetterMapsSpeedMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapSpeedConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption CrewVisionMultiplier { get; set; } =
        new("TouOptionBetterMapsCrewVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCrewVisionConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption ImpVisionMultiplier { get; set; } =
        new("TouOptionBetterMapsImpVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapImpVisionConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption CooldownOffset { get; set; } =
        new("TouOptionBetterMapsCooldownOffset", 0f, -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCooldownConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption OffsetShortTasks { get; set; } =
        new("TouOptionBetterMapsOffsetShortTasks", 0f, -5f, 5f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TouOptionBetterMapsOffsetLongTasks", 0f, -3f, 3f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapLongTaskConfig) ==
                MapTweakMode.GlobalOn
        };
}

public enum MapTweakMode
{
    GlobalOff,
    GlobalOn,
    PerMap
}