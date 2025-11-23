using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PlumberOptions : AbstractOptionGroup<PlumberRole>
{
    public override string GroupName => TouLocale.Get("TouRolePlumber", "Plumber");

    [ModdedNumberOption("TouOptionPlumberFlushCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float FlushCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionPlumberBlockCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float BlockCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionPlumberMaxNumberOfBarricades", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxBarricades { get; set; } = 3f;

    [ModdedNumberOption("TouOptionPlumberAmountOfRoundsBarricadesLast", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float BarricadeRoundDuration { get; set; } = 2f;

    [ModdedToggleOption("TouOptionPlumberGetMoreFromTasks")]
    public bool TaskUses { get; set; } = true;
}