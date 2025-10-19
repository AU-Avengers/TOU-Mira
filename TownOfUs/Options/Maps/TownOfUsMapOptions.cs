using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;

namespace TownOfUs.Options.Maps;

public sealed class TownOfUsMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Map Options";
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

    [ModdedToggleOption("Half Vision On Skeld/Mira")]
    public bool SmallMapHalfVision { get; set; } = false;

    [ModdedNumberOption("Mira HQ Decreased Cooldowns", 0f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SmallMapDecreasedCooldown { get; set; } = 0f;

    [ModdedNumberOption("Airship/Submerged Increased Cooldowns", 0f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float LargeMapIncreasedCooldown { get; set; } = 0f;

    [ModdedNumberOption("Skeld/Mira Increased Short Tasks", 0f, 5f)]
    public float SmallMapIncreasedShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Skeld/Mira Increased Long Tasks", 0f, 3f)]
    public float SmallMapIncreasedLongTasks { get; set; } = 0f;

    [ModdedNumberOption("Airship/Submerged Decreased Short Tasks", 0f, 5f)]
    public float LargeMapDecreasedShortTasks { get; set; } = 0f;

    [ModdedNumberOption("Airship/Submerged Decreased Long Tasks", 0f, 3f)]
    public float LargeMapDecreasedLongTasks { get; set; } = 0f;

    // MapNames 6 is Submerged
    public float GetMapBasedCooldownDifference()
    {
        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            MapNames.MiraHQ => -SmallMapDecreasedCooldown,
            MapNames.Airship or (MapNames)6 => LargeMapIncreasedCooldown,
            _ => 0
        };
    }

    public int GetMapBasedShortTasks()
    {
        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            MapNames.MiraHQ or MapNames.Skeld or MapNames.Dleks => (int)SmallMapIncreasedShortTasks,
            MapNames.Airship or (MapNames)6 => -(int)LargeMapDecreasedShortTasks,
            _ => 0
        };
    }

    public int GetMapBasedLongTasks()
    {
        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            MapNames.MiraHQ or MapNames.Skeld or MapNames.Dleks => (int)SmallMapIncreasedLongTasks,
            MapNames.Airship or (MapNames)6 => -(int)LargeMapDecreasedLongTasks,
            _ => 0
        };
    }
}