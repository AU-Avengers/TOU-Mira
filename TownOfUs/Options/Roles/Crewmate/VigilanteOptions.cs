using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class VigilanteOptions : AbstractOptionGroup<VigilanteRole>
{
    public override string GroupName => TouLocale.Get("TouRoleVigilante", "Vigilante");

    [ModdedNumberOption("TouOptionVigilanteNumberOfGuesses", 1f, 15f)]
    public float VigilanteKills { get; set; } = 5f;

    [ModdedToggleOption("TouOptionVigilanteCanGuessMoreThanOncePerMeeting")]
    public bool VigilanteMultiKill { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessNeutralBenignRoles")]
    public bool VigilanteGuessNeutralBenign { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessNeutralEvilRoles")]
    public bool VigilanteGuessNeutralEvil { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessNeutralKillingRoles")]
    public bool VigilanteGuessNeutralKilling { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessNeutralOutlierRoles")]
    public bool VigilanteGuessNeutralOutlier { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessKillerModifiers")]
    public bool VigilanteGuessKillerMods { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessAlliances")]
    public bool VigilanteGuessAlliances { get; set; } = true;

    [ModdedNumberOption("TouOptionVigilanteSafeShotsAvailable", 0f, 3f, 1f, MiraNumberSuffixes.None, "0")]
    public float MultiShots { get; set; } = 3;
}