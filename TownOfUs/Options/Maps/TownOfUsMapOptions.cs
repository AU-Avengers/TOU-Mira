using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class TownOfUsMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.RandomMapChoice");
    public override uint GroupPriority => 0;
    public override bool ShowInMatchInfoGuide => RandomMaps;
    public override string MatchInfoGuidePageKey => "TownOfUsMira.OptionGroups.BetterMaps";
    public override string MatchInfoGuidePageTitle => MiraLocaleManager.Get("WikiSettingsBetterMapsSettingsTitle");
    public override LoadableAsset<Sprite> Sprite => TouAssets.AdminSprite;

    [ModdedToggleOption("TownOfUsMira.RandomMaps.Option.Toggle")]
    public bool RandomMaps { get; set; } = false;

    public ExtendedNumberOption SkeldChance { get; } = new("TownOfUsMira.RandomMaps.Option.SkeldChance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: new Color32(188, 206, 200, 255), asset: TouAssets.IconSkeld,
        assetName: "AmongUs.Map.Skeld", assetScale: 1.45f)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ExtendedNumberOption BackwardsSkeldChance { get; } = new("TownOfUsMira.RandomMaps.Option.BackwardsSkeldChance", 0, 0,
        100f, 10f, "#", "#", MiraNumberSuffixes.Percent, color: new Color32(188, 206, 200, 255),
        asset: TouAssets.IconDleks, assetName: "AmongUs.Map.dlekS", assetScale: 1.45f)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ExtendedNumberOption MiraChance { get; } = new("TownOfUsMira.RandomMaps.Option.MiraChance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: new Color32(255, 128, 100, 255), asset: TouAssets.IconMira,
        assetName: "AmongUs.Map.Mira", assetScale: 1.45f)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ExtendedNumberOption PolusChance { get; } = new("TownOfUsMira.RandomMaps.Option.PolusChance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: new Color32(157, 146, 198, 255), asset: TouAssets.IconPolus,
        assetName: "AmongUs.Map.Polus", assetScale: 1.45f)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ExtendedNumberOption AirshipChance { get; } =
        new("TownOfUsMira.RandomMaps.Option.AirshipChance", 0, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: new Color32(255, 76, 73, 255), asset: TouAssets.IconAirship, assetName: "AmongUs.Map.Airship",
            assetScale: 1.45f)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
        };

    public ExtendedNumberOption FungleChance { get; } = new("TownOfUsMira.RandomMaps.Option.FungleChance", 0, 0, 100f, 10f, "#",
        "#", MiraNumberSuffixes.Percent, color: new Color32(239, 98, 162, 255), asset: TouAssets.IconFungle,
        assetName: "AmongUs.Map.Fungle", assetScale: 1.45f)
    {
        Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps
    };

    public ExtendedNumberOption SubmergedChance { get; } =
        new("TownOfUsMira.RandomMaps.Option.SubmergedChance", 0, 0f, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: new Color32(10, 150, 255, 255), asset: TouAssets.IconSubmerged, assetName: "AmongUs.Map.Submerged",
            assetScale: 1.45f)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps && ModCompatibility.SubLoaded
        };

    public ExtendedNumberOption LevelImpostorChance { get; } =
        new("TownOfUsMira.RandomMaps.Option.LevelImpostorChance", 0, 0f, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: new Color32(16, 131, 176, 255), asset: TouAssets.IconLevelImposter, assetName: "AmongUs.Map.LevelImposter",
            assetScale: 1.45f)
        {
            Visible = () => OptionGroupSingleton<TownOfUsMapOptions>.Instance.RandomMaps && ModCompatibility.LILoaded
        };

    public static float GetMapBasedSpeedMultiplier()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapImpVisionConfig;
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
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance
                .SpeedMultiplier,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.SpeedMultiplier,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.SpeedMultiplier
                ,
            _ => 1
        };
    }

    public static float GetMapBasedCrewmateVision()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapCrewVisionConfig;
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
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance
                .CrewVisionMultiplier,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CrewVisionMultiplier,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CrewVisionMultiplier
                ,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance
                .CrewVisionMultiplier,
            _ => 1
        };
    }

    public static float GetMapBasedImpostorVision()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapImpVisionConfig;
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
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapCooldownConfig;
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
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance
                .CooldownOffset,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CooldownOffset,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CooldownOffset,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CooldownOffset
                ,
            _ => 0
        };
        return offset - EgotistModifier.CooldownReduction;
    }

    public static int GetMapBasedShortTasks()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapShortTaskConfig;
        if (mode == MapTweakMode.GlobalOn)
        {
            return opts.OffsetShortTasks;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 0;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.OffsetShortTasks,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.OffsetShortTasks,
            _ => 0
        };
    }

    public static int GetMapBasedLongTasks()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapLongTaskConfig;
        if (mode == MapTweakMode.GlobalOn)
        {
            return opts.OffsetLongTasks;
        }

        if (mode == MapTweakMode.GlobalOff)
        {
            return 0;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.OffsetLongTasks,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.OffsetLongTasks,
            _ => 0
        };
    }

    public static bool IsCamoCommsOn()
    {
        var opts = OptionGroupSingleton<GlobalBetterMapOptions>.Instance;
        var mode = opts.GlobalMapCamoCommsConfig;
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
            ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => OptionGroupSingleton<BetterSkeldOptions>.Instance
                .CamoComms,
            ExpandedMapNames.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.CamoComms,
            ExpandedMapNames.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.CamoComms,
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.CamoComms,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.CamoComms,
            ExpandedMapNames.Submerged => OptionGroupSingleton<BetterSubmergedOptions>.Instance.CamoComms,
            ExpandedMapNames.LevelImpostor => OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.CamoComms,
            _ => false
        };
    }

    public static bool AreLadderCooldownsDisabled()
    {
        if (GameOptionsManager.Instance == null || !AmongUsClient.Instance || !PlayerControl.LocalPlayer)
        {
            return false;
        }

        return MiscUtils.GetCurrentMap switch
        {
            ExpandedMapNames.Airship => OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown,
            ExpandedMapNames.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.NoLadderCooldown,
            ExpandedMapNames.LevelImpostor =>
                OptionGroupSingleton<BetterLevelImpostorOptions>.Instance.NoLadderCooldown,
            _ => false
        };
    }
}