using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PlumberOptions : AbstractOptionGroup<PlumberRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Plumber, "Plumber");

    [ModdedNumberOption("Flush Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float FlushCooldown { get; set; } = 25f;

    [ModdedNumberOption("Block Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float BlockCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Number Of Barricades", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxBarricades { get; set; } = 3f;
    
    [ModdedNumberOption("Amount Of Rounds Barricades Last", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float BarricadeRoundDuration { get; set; } = 2f;

    [ModdedToggleOption("Get More Barricades From Completing Tasks")]
    public bool TaskUses { get; set; } = true;
}