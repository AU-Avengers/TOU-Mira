using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class BetterAirshipOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Airship";
    public override uint GroupPriority => 5;

    [ModdedToggleOption("Airship Doors Are Polus Doors")]
    public bool AirshipPolusDoors { get; set; } = false;

    [ModdedEnumOption("Spawn Mode", typeof(SpawnModes), ["Normal", "Everyone Has Same Spawns", "Host Chooses One"])]
    public SpawnModes SpawnMode { get; set; } = SpawnModes.Normal;

    public ModdedEnumOption SingleLocation { get; } = new ModdedEnumOption("Spawn At", 0, typeof(Locations),
        ["Main Hall", "Kitchen", "Cargo Bay", "Engine Room", "Brig", "Records"])
    {
        Visible = () => OptionGroupSingleton<BetterAirshipOptions>.Instance.SpawnMode == SpawnModes.HostChoosesOne,
    };

    [ModdedNumberOption("Crash Course Sabotage Countdown", 15f, 90f, 5f, MiraNumberSuffixes.Seconds)]
    public float SaboCountdownReactor { get; set; } = 90f;

    public enum SpawnModes
    {
        Normal,
        SameSpawns,
        HostChoosesOne
    }

    public enum Locations
    {
        MainHall,
        Kitchen,
        CargoBay,
        EngineRoom,
        Brig,
        Records,
    }
}