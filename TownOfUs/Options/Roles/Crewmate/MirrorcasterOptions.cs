using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MirrorcasterOptions : AbstractOptionGroup<MirrorcasterRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Mirrorcaster, "Mirrorcaster");

    [ModdedEnumOption("Who Gets Murder Attempt Indicator", typeof(MirrorOption), ["Mirrorcaster", "Mirrorcaster + Killer"])]
    public MirrorOption WhoGetsNotification { get; set; } = MirrorOption.MirrorcasterAndKiller;

    public ModdedNumberOption MirrorCooldown { get; } =
        new($"Magic Mirror Cooldown", 0f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption MirrorDuration { get; } =
        new($"Magic Mirror Duration", 30f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption UnleashCooldown { get; } =
        new($"Unleash Cooldown", 15f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds);
    
    [ModdedToggleOption("Mirrorcaster Knows The Absorbed Type Of Attack")]
    public bool KnowAttackType { get; set; } = true;
    
    [ModdedToggleOption("Accumulate Multiple Unleashes")]
    public bool MultiUnleash { get; set; } = false;
    
    [ModdedNumberOption("Max Number Of Magic Mirrors", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxMirrors { get; set; } = 5f;
}

public enum MirrorOption
{
    Mirrorcaster,
    MirrorcasterAndKiller
}