using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class GuardianOptions : AbstractOptionGroup<GuardianRole>
{
    public override string GroupName => "Guardian";

    [ModdedNumberOption("Aegis Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AegisCooldown { get; set; } = 20;
    
    [ModdedToggleOption("Target Sees Aegis")]
    public bool TargetSeesAegis { get; set; } = false;
}