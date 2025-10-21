using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class TownOfUsMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Random Map Choice";
    public override uint GroupPriority => 0;

    [ModdedToggleOption("Enable Random Maps")]
    public bool RandomMaps { get; set; } = false;

    public ModdedNumberOption SkeldChance { get; } = new("Skeld Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption BackwardsSkeldChance { get; } = new("dlekS ecnahC", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption MiraChance { get; } = new("Mira Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption PolusChance { get; } = new("Polus Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption AirshipChance { get; } =
        new("Airship Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
        };

    public ModdedNumberOption FungleChance { get; } = new("Fungle Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption SubmergedChance { get; } =
        new("Submerged Chance", 0, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps && ModCompatibility.SubLoaded
        };

    public ModdedNumberOption LevelImpostorChance { get; } =
        new("Level Impostor Chance", 0, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps && ModCompatibility.LILoaded
        };

    public static float GetMapBasedSpeedMultiplier()
    {
        return (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.SpeedMultiplier,
            _ => 1
        };
    }

    public static float GetMapBasedCrewmateVision()
    {
        return (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CrewVisionMultiplier,
            _ => 1
        };
    }

    public static float GetMapBasedImpostorVision()
    {
        return (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.ImpVisionMultiplier,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.ImpVisionMultiplier,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.ImpVisionMultiplier,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.ImpVisionMultiplier,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.ImpVisionMultiplier,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.ImpVisionMultiplier,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.ImpVisionMultiplier,
            _ => 1
        };
    }

    public static float GetMapBasedCooldownDifference()
    {
        return (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.CooldownOffset,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CooldownOffset,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CooldownOffset,
            _ => 0
        };
    }

    public static int GetMapBasedShortTasks()
    {
        return (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => (int)OptionGroupSingleton<BetterSkeldOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.MiraHq => (int)OptionGroupSingleton<BetterMiraHqOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Polus => (int)OptionGroupSingleton<BetterPolusOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Airship => (int)OptionGroupSingleton<BetterAirshipOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Fungle => (int)OptionGroupSingleton<BetterFungleOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Submerged => (int)OptionGroupSingleton<BetterSubmergedOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.LevelImpostor => (int)OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.OffsetShortTasks,
            _ => 0
        };
    }

    public static int GetMapBasedLongTasks()
    {
        return (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => (int)OptionGroupSingleton<BetterSkeldOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.MiraHq => (int)OptionGroupSingleton<BetterMiraHqOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Polus => (int)OptionGroupSingleton<BetterPolusOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Airship => (int)OptionGroupSingleton<BetterAirshipOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Fungle => (int)OptionGroupSingleton<BetterFungleOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Submerged => (int)OptionGroupSingleton<BetterSubmergedOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.LevelImpostor => (int)OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.OffsetLongTasks,
            _ => 0
        };
    }
}