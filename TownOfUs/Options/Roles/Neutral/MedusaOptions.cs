using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
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

    public ModdedToggleOption StoneGazeAvailable { get; set; } = new("Allow Stone Gazing", true);
    
    public ModdedNumberOption StoneGazeCooldown { get; set; } = new("Stone Gaze Cooldown", 35f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable
    };
    
    public ModdedNumberOption StoneGazeDuration { get; set; } = new("Stone Gaze Duration", 10f, 5f, 20f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable
    };
    
    public ModdedNumberOption StoneGazeUses { get; set; } = new("Stone Gaze Uses", 3f, 1f, 10f, 1f, MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable
    };

    [ModdedToggleOption("TouOptionMedusaCanVent")]
    public bool CanVent { get; set; } = false;
}