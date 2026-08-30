using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class GlobalBetterMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.GlobalBetterMaps");
    public override uint GroupPriority => 0;
    public static MapTweakMode GetMapTweakMode(ModdedEnumOption option) => (MapTweakMode)option.Value;

    public static readonly string[] GlobalOpts =
    [
        "TownOfUsMira.GlobalBetterMap.Option.ChangeEnumOff", "TownOfUsMira.GlobalBetterMap.Option.ChangeEnumOn",
        "TownOfUsMira.GlobalBetterMap.Option.ChangeEnumPerMap"
    ];

    public ModdedEnumOption GlobalMapCamoCommsConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.CamouflageComms",
        (int)MapTweakMode.GlobalOff, typeof(MapTweakMode), GlobalOpts);

    public ModdedEnumOption GlobalMapSpeedConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.SpeedMultiplier",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode), GlobalOpts);

    public ModdedEnumOption GlobalMapCrewVisionConfig { get; set; } = new(
        "TownOfUsMira.GlobalBetterMap.Option.CrewVisionMultiplier", (int)MapTweakMode.PerMap, typeof(MapTweakMode), GlobalOpts);

    public ModdedEnumOption GlobalMapImpVisionConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.ImpVisionMultiplier",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode), GlobalOpts);

    public ModdedEnumOption GlobalMapCooldownConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.CooldownOffset",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode), GlobalOpts);

    public ModdedEnumOption GlobalMapShortTaskConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.OffsetShortTasks",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode), GlobalOpts);

    public ModdedEnumOption GlobalMapLongTaskConfig { get; set; } = new("TownOfUsMira.GlobalBetterMap.Option.OffsetLongTasks",
        (int)MapTweakMode.PerMap, typeof(MapTweakMode), GlobalOpts);

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsSpeedMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapSpeedConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption CrewVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCrewVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCrewVisionConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption ImpVisionMultiplier { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsImpVisionMultiplier", 1f, 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapImpVisionConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption CooldownOffset { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsCooldownOffset", 0f, -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapCooldownConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption OffsetShortTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetShortTasks", 0f, -5f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () =>
                GetMapTweakMode(OptionGroupSingleton<GlobalBetterMapOptions>.Instance.GlobalMapShortTaskConfig) ==
                MapTweakMode.GlobalOn
        };

    public ModdedNumberOption OffsetLongTasks { get; set; } =
        new("TownOfUsMira.BetterMaps.Option.MapsOffsetLongTasks", 0f, -3f, 3f, 1f, MiraNumberSuffixes.None)
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