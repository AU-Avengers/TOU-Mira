using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class AmbusherOptions : AbstractOptionGroup<AmbusherRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Ambusher, "Ambusher");

    [ModdedNumberOption("Ambush Uses Per Game", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxAmbushes { get; set; } = 0f;
    
    [ModdedNumberOption("Ambush Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AmbushCooldown { get; set; } = 25f;
    
    [ModdedNumberOption("Pursue Arrow Update Interval", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float UpdateInterval { get; set; } = 2.5f;

    [ModdedToggleOption("Stop Pursing Player On Ambush")]
    public bool ResetAmbush { get; set; } = true;

    [ModdedToggleOption("Ambusher Can Vent")]
    public bool CanVent { get; set; } = true;
}