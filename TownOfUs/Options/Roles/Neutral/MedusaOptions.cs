using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class MedusaOptions : AbstractRoleOptionGroup<MedusaRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Medusa", "Medusa");

    [ModdedNumberOption("TouOptionMedusaPetrifyCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionMedusaStoneDelay", 5f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float StoneDelay { get; set; } = 10f;

    [ModdedNumberOption("TouOptionMedusaStoneCompletion", 12.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float StoneCompletion { get; set; } = 20f;

    public ModdedToggleOption StoneGazeAvailable { get; set; } = new("TouOptionMedusaAllowStoneGazing", true);
    
    public ModdedNumberOption StoneGazeCooldown { get; set; } = new("TouOptionMedusaStoneGazeCooldown", 35f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable
    };
    
    public ModdedNumberOption StoneGazeDuration { get; set; } = new("TouOptionMedusaStoneGazeDuration", 10f, 5f, 20f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable
    };
    
    public ModdedNumberOption StoneGazeUses { get; set; } = new("TouOptionMedusaStoneGazeUses", 3f, 1f, 10f, 1f, MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable
    };

    [ModdedToggleOption("TouOptionMedusaCanVent")]
    public bool CanVent { get; set; } = false;
}