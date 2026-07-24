using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class MedusaOptions : AbstractRoleOptionGroup<MedusaRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMedusa", "Medusa");

    [ModdedNumberOption("TouOptionMedusaPetrifyCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("Time For Victim To Become Stoned", 5f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float StoneDelay { get; set; } = 10f;

    [ModdedNumberOption("Time Before Stone Shatters", 12.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float StoneCompletion { get; set; } = 20f;

    [ModdedToggleOption("TouOptionMedusaPetrifyFirstRound")]
    public bool FirstRound { get; set; } = false;

    [ModdedToggleOption("TouOptionMedusaCanVent")]
    public bool CanVent { get; set; } = false;
}