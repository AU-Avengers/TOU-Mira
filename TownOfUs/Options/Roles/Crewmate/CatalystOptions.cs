using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class CatalystOptions : AbstractOptionGroup<CatalystRole>
{
    public override string GroupName => "Catalyst";

    [ModdedNumberOption("Overcharge Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds, formatString: "0.0")]
    public float OverchargeCooldown { get; set; } = 5f;
    
    [ModdedNumberOption("Overcharge Uses Per Round", 0, 15, 1, zeroInfinity: true)]
    public float OverchargeUses { get; set; } = 1;
    
    [ModdedNumberOption("Overcharge Cooldown Decrease Multiplier", 1.1f, 3, 0.1f, MiraNumberSuffixes.Multiplier, "0.0")]
    public float OverchargedMultiplier { get; set; } = 2f;
}