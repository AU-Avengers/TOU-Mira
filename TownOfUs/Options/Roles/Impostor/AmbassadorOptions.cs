using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class AmbassadorOptions : AbstractOptionGroup<AmbassadorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleAmbassador", "Ambassador");

    [ModdedNumberOption("TouOptionAmbassadorMaxRetrainsAvailable", 1, 3)]
    public float MaxRetrains { get; set; } = 2f;

    [ModdedToggleOption("TouOptionAmbassadorRetrainRequiresConfirmation")]
    public bool RetrainConfirmation { get; set; } = true;

    [ModdedNumberOption("TouOptionAmbassadorKillsNeededByAmbassadorOrTeammateToRetrain", 0, 4)]
    public float KillsNeeded { get; set; } = 2f;

    [ModdedNumberOption("TouOptionAmbassadorRoundInWhichRetrainingIsPossible", 1, 5)]
    public float RoundWhenAvailable { get; set; } = 2f;

    public ModdedNumberOption RoundCooldown { get; } =
        new("TouOptionAmbassadorRoundsNeededToRetrainAgain", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => (int)OptionGroupSingleton<AmbassadorOptions>.Instance.MaxRetrains > 1
        };
}