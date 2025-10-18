using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TownOfUs.Options;

public sealed class AirshipOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Airship";
    public override uint GroupPriority => 6;

    [ModdedToggleOption("Airship Doors Are Polus Doors")]
    public bool AirshipPolusDoors { get; set; } = false;

    [ModdedEnumOption("Spawn Mode", typeof(SpawnModes), ["Normal", "Everyone Has Same Spawns", "Host Chooses One"])]
    public SpawnModes SpawnMode { get; set; } = SpawnModes.Normal;

    public ModdedEnumOption SingleLocation { get; } = new ModdedEnumOption("Spawn At", 0, typeof(Locations),
        ["Main Hall", "Kitchen", "Cargo Bay", "Engine Room", "Brig", "Records"])
    {
        Visible = () => OptionGroupSingleton<AirshipOptions>.Instance.SpawnMode == SpawnModes.HostChoosesOne,
    };

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