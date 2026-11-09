using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Options.Maps;

public sealed class RandomDoorMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.RandomDoorMode");
    public override uint GroupPriority => 1;
    public override bool ShowInMatchInfoGuide => true;
    public override string MatchInfoGuidePageKey => "TownOfUsMira.OptionGroups.BetterMaps";
    public override string MatchInfoGuidePageTitle => MiraLocaleManager.Get("WikiSettingsBetterMapsSettingsTitle");
    public override LoadableAsset<Sprite> Sprite => TouAssets.AdminSprite;

    public ModdedNumberOption DisabledDoorChance { get; } = new("TownOfUsMira.BetterMaps.Option.DoorsNoneChance", 5f, 0, 100f, 10f, MiraNumberSuffixes.Percent);
    public ModdedNumberOption SkeldDoorChance { get; } = new("TownOfUsMira.BetterMaps.Option.DoorsSkeldChance", 20f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption PolusDoorChance { get; } = new("TownOfUsMira.BetterMaps.Option.DoorsPolusChance", 40f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption AirshipDoorChance { get; } = new("TownOfUsMira.BetterMaps.Option.DoorsAirshipChance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption FungleDoorChance { get; } = new("TownOfUsMira.BetterMaps.Option.DoorsFungleChance", 20f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption SubmergedDoorChance { get; } = new("TownOfUsMira.BetterMaps.Option.DoorsSubmergedChance", 25f, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => ModCompatibility.SubLoaded
    };

    public static MapDoorType GetRandomDoorType(MapDoorType defaultDoor)
    {
        int disabledChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.DisabledDoorChance;
        int skeldChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.SkeldDoorChance;
        int polusChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.PolusDoorChance;
        int airshipChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.AirshipDoorChance;
        int fungleChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.FungleDoorChance;
        int submergedChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.SubmergedDoorChance;

        Random rnd = new();
        int totalWeight = 0;

        totalWeight += skeldChance;
        totalWeight += polusChance;
        totalWeight += disabledChance;
        totalWeight += airshipChance;
        totalWeight += fungleChance;

        totalWeight += ModCompatibility.SubLoaded ? submergedChance : 0;

        if (totalWeight == 0)
        {
            return defaultDoor;
        }

        float randomNumber = rnd.Next(0, totalWeight);

        if (randomNumber < skeldChance)
        {
            return MapDoorType.Skeld;
        }

        randomNumber -= skeldChance;

        if (randomNumber < polusChance)
        {
            return MapDoorType.Polus;
        }

        randomNumber -= polusChance;

        if (randomNumber < disabledChance)
        {
            return MapDoorType.None;
        }

        randomNumber -= disabledChance;

        if (randomNumber < airshipChance)
        {
            return MapDoorType.Airship;
        }

        randomNumber -= airshipChance;

        if (randomNumber < fungleChance)
        {
            return MapDoorType.Fungle;
        }

        randomNumber -= fungleChance;

        if (ModCompatibility.SubLoaded && randomNumber < submergedChance)
        {
            return MapDoorType.Submerged;
        }

        return defaultDoor;
    }
}

public enum MapDoorType
{
    Skeld,
    Polus,
    Airship,
    Fungle,
    Submerged, // This is just cause it would be cool to implement, if submerged isn't installed, it will autoset to the default
    None, // Disables all doors, like Mira HQ
    Random, // This is used when the door chance is applicable
}