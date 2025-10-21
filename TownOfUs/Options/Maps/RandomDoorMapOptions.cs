using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules;

namespace TownOfUs.Options.Maps;

public sealed class RandomDoorMapOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Randomized Door Mode";
    public override uint GroupPriority => 1;

    public ModdedNumberOption SkeldDoorChance { get; } = new("Skeld Door Chance", 25f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption PolusDoorChance { get; } = new("Polus Door Chance", 25f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption AirshipDoorChance { get; } = new("Airship Door Chance", 25f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption FungleDoorChance { get; } = new("Fungle Door Chance", 25f, 0, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption SubmergedDoorChance { get; } = new("Submerged Door Chance", 25f, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => ModCompatibility.SubLoaded
    };
    
    public static MapDoorType GetRandomDoorType(MapDoorType defaultDoor)
    {
        var skeldChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.SkeldDoorChance.Value;
        var polusChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.PolusDoorChance.Value;
        var airshipChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.AirshipDoorChance.Value;
        var fungleChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.FungleDoorChance.Value;
        var submergedChance = OptionGroupSingleton<RandomDoorMapOptions>.Instance.SubmergedDoorChance.Value;

        Random rnd = new();
        float totalWeight = 0;

        totalWeight += skeldChance;
        totalWeight += polusChance;
        totalWeight += airshipChance;
        totalWeight += fungleChance;

        totalWeight += ModCompatibility.SubLoaded ? submergedChance : 0;

        if ((int)totalWeight == 0)
        {
            return defaultDoor;
        }

        float randomNumber = rnd.Next(0, (int)totalWeight);

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
    Random, // This is used when the door chance is applicable
    Skeld,
    Polus,
    Airship,
    Fungle,
    Submerged, // This is just cause it would be cool to implement, if submerged isn't installed, it will autoset to the default
}