using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class TownOfUsMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Random Map Choice";
    public override uint GroupPriority => 0;

    [ModdedToggleOption("TouOptionRandomMapsToggle")]
    public bool RandomMaps { get; set; } = false;

    public ModdedNumberOption SkeldChance { get; } = new("TouOptionRandomMapsSkeldChance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption BackwardsSkeldChance { get; } = new("TouOptionRandomMapsBackwardsSkeldChance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption MiraChance { get; } = new("TouOptionRandomMapsMiraChance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption PolusChance { get; } = new("TouOptionRandomMapsPolusChance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption AirshipChance { get; } =
        new("TouOptionRandomMapsAirshipChance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
        };

    public ModdedNumberOption FungleChance { get; } = new("TouOptionRandomMapsFungleChance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ModdedNumberOption SubmergedChance { get; } =
        new("TouOptionRandomMapsSubmergedChance", 0, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps && ModCompatibility.SubLoaded
        };

    public ModdedNumberOption LevelImpostorChance { get; } =
        new("TouOptionRandomMapsLevelImpostorChance", 0, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps && ModCompatibility.LILoaded
        };

    public static float GetMapBasedSpeedMultiplier()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapImpVisionConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return opts.SpeedMultiplier.Value;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 1;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.SpeedMultiplier.Value,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.SpeedMultiplier.Value,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.SpeedMultiplier.Value,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpeedMultiplier.Value,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.SpeedMultiplier.Value,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.SpeedMultiplier.Value,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.SpeedMultiplier.Value,
            _ => 1
        };
    }

    public static float GetMapBasedCrewmateVision()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapCrewVisionConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return opts.CrewVisionMultiplier.Value;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 1;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.CrewVisionMultiplier.Value,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CrewVisionMultiplier.Value,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CrewVisionMultiplier.Value,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CrewVisionMultiplier.Value,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CrewVisionMultiplier.Value,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CrewVisionMultiplier.Value,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CrewVisionMultiplier.Value,
            _ => 1
        };
    }

    public static float GetMapBasedImpostorVision()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapImpVisionConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return opts.ImpVisionMultiplier.Value;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 1;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.ImpVisionMultiplier.Value,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.ImpVisionMultiplier.Value,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.ImpVisionMultiplier.Value,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.ImpVisionMultiplier.Value,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.ImpVisionMultiplier.Value,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.ImpVisionMultiplier.Value,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.ImpVisionMultiplier.Value,
            _ => 1
        };
    }

    public static float GetMapBasedCooldownDifference()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapCooldownConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return opts.CooldownOffset.Value - EgotistModifier.CooldownReduction;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return -EgotistModifier.CooldownReduction;
        }

        var offset = MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.CooldownOffset.Value,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CooldownOffset.Value,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CooldownOffset.Value,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CooldownOffset.Value,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CooldownOffset.Value,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CooldownOffset.Value,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CooldownOffset.Value,
            _ => 0
        };
        return offset - EgotistModifier.CooldownReduction;
    }

    public static int GetMapBasedShortTasks()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapShortTaskConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return (int)opts.OffsetShortTasks.Value;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 0;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => (int)OptionGroupSingleton<BetterSkeldOptions>.Instance.OffsetShortTasks.Value,
            ExpandedMapNames.MiraHq => (int)OptionGroupSingleton<BetterMiraHqOptions>.Instance.OffsetShortTasks.Value,
            ExpandedMapNames.Polus => (int)OptionGroupSingleton<BetterPolusOptions>.Instance.OffsetShortTasks.Value,
            ExpandedMapNames.Airship => (int)OptionGroupSingleton<BetterAirshipOptions>.Instance.OffsetShortTasks.Value,
            ExpandedMapNames.Fungle => (int)OptionGroupSingleton<BetterFungleOptions>.Instance.OffsetShortTasks.Value,
            ExpandedMapNames.Submerged => (int)OptionGroupSingleton<BetterSubmergedOptions>.Instance.OffsetShortTasks.Value,
            ExpandedMapNames.LevelImpostor => (int)OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.OffsetShortTasks.Value,
            _ => 0
        };
    }

    public static int GetMapBasedLongTasks()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapLongTaskConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return (int)opts.OffsetLongTasks.Value;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 0;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => (int)OptionGroupSingleton<BetterSkeldOptions>.Instance.OffsetLongTasks.Value,
            ExpandedMapNames.MiraHq => (int)OptionGroupSingleton<BetterMiraHqOptions>.Instance.OffsetLongTasks.Value,
            ExpandedMapNames.Polus => (int)OptionGroupSingleton<BetterPolusOptions>.Instance.OffsetLongTasks.Value,
            ExpandedMapNames.Airship => (int)OptionGroupSingleton<BetterAirshipOptions>.Instance.OffsetLongTasks.Value,
            ExpandedMapNames.Fungle => (int)OptionGroupSingleton<BetterFungleOptions>.Instance.OffsetLongTasks.Value,
            ExpandedMapNames.Submerged => (int)OptionGroupSingleton<BetterSubmergedOptions>.Instance.OffsetLongTasks.Value,
            ExpandedMapNames.LevelImpostor => (int)OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.OffsetLongTasks.Value,
            _ => 0
        };
    }

    public static bool IsCamoCommsOn()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = GlobalBetterMapOptions.GetMapTweakMode(opts.GlobalMapCamoCommsConfig);
        if (mode == MapTweakMode.GlobalOn)
        {
            return true;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return false;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.CamoComms.Value,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CamoComms.Value,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CamoComms.Value,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CamoComms.Value,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CamoComms.Value,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CamoComms.Value,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CamoComms.Value,
            _ => false
        };
    }

    public static bool AreLadderCooldownsDisabled()
    {
        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.NoLadderCooldown,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.NoLadderCooldown,
            _ => false
        };
    }
}