using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;

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

    // MapNames 6 is Submerged
    public static float GetMapBasedCooldownDifference()
    {
        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            MapNames.MiraHQ => -OptionGroupSingleton<BetterMiraHqOptions>.Instance.CooldownDecrease,
            MapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CooldownIncrease,
            (MapNames)6 => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CooldownIncrease,
            _ => 0
        };
    }

    public static int GetMapBasedShortTasks()
    {
        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            MapNames.Skeld or MapNames.Dleks => (int)OptionGroupSingleton<BetterSkeldOptions>.Instance.IncreasedShortTasks,
            MapNames.MiraHQ => (int)OptionGroupSingleton<BetterMiraHqOptions>.Instance.IncreasedShortTasks,
            MapNames.Airship => -(int)OptionGroupSingleton<BetterAirshipOptions>.Instance.DecreasedShortTasks,
            (MapNames)6 => -(int)OptionGroupSingleton<BetterSubmergedOptions>.Instance.DecreasedShortTasks,
            _ => 0
        };
    }

    public static int GetMapBasedLongTasks()
    {
        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            MapNames.Skeld or MapNames.Dleks => (int)OptionGroupSingleton<BetterSkeldOptions>.Instance.IncreasedLongTasks,
            MapNames.MiraHQ => (int)OptionGroupSingleton<BetterMiraHqOptions>.Instance.IncreasedLongTasks,
            MapNames.Airship => -(int)OptionGroupSingleton<BetterAirshipOptions>.Instance.DecreasedLongTasks,
            (MapNames)6 => -(int)OptionGroupSingleton<BetterSubmergedOptions>.Instance.DecreasedLongTasks,
            _ => 0
        };
    }
}