using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class VigilanteOptions : AbstractOptionGroup<VigilanteRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleVigilante", "Vigilante");

    [ModdedNumberOption("TouOptionVigilanteNumberOfGuesses", 1f, 15f)]
    public float VigilanteKills { get; set; } = 5f;

    [ModdedToggleOption("TouOptionVigilanteCanGuessMoreThanOncePerMeeting")]
    public bool VigilanteMultiKill { get; set; } = true;

    public ModdedToggleOption VigilanteGuessNeutralBenign { get; set; } = new("TouOptionVigilanteCanGuessNeutralBenignRoles", true);

    public ModdedToggleOption VigilanteGuessNeutralEvil { get; set; } = new("TouOptionVigilanteCanGuessNeutralEvilRoles", true);

    public ModdedToggleOption VigilanteGuessNeutralKilling { get; set; } = new("TouOptionVigilanteCanGuessNeutralKillingRoles", true);

    public ModdedToggleOption VigilanteGuessNeutralOutlier { get; set; } = new("TouOptionVigilanteCanGuessNeutralOutlierRoles", true);

    [ModdedToggleOption("TouOptionVigilanteCanGuessKillerModifiers")]
    public bool VigilanteGuessKillerMods { get; set; } = true;

    [ModdedToggleOption("TouOptionVigilanteCanGuessAlliances")]
    public bool VigilanteGuessAlliances { get; set; } = true;

    [ModdedNumberOption("TouOptionVigilanteSafeShotsAvailable", 0f, 3f, 1f, MiraNumberSuffixes.None, "0")]
    public float MultiShots { get; set; } = 3;
    
    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            VigilanteGuessNeutralBenign.StringName,
            VigilanteGuessNeutralEvil.StringName,
            VigilanteGuessNeutralKilling.StringName,
            VigilanteGuessNeutralOutlier.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var title = TouLocale.GetParsed("TouOptionVigilanteGuessableNeutrals");
        var nbValid = VigilanteGuessNeutralBenign.Value;
        var neValid = VigilanteGuessNeutralEvil.Value;
        var nkValid = VigilanteGuessNeutralKilling.Value;
        var noValid = VigilanteGuessNeutralOutlier.Value;

        if (!nbValid && !neValid && !nkValid && !noValid)
        {
            var newArray = new []
                { $"{title}: {TouLocale.GetParsed("TouOptionVigilanteGuessableNone")}" };
            return newArray;
        }

        var selected = new List<string>();
        if (nbValid) selected.Add(TouLocale.GetParsed("TouOptionVigilanteGuessableBenign"));
        if (neValid) selected.Add(TouLocale.GetParsed("TouOptionVigilanteGuessableEvil"));
        if (nkValid) selected.Add(TouLocale.GetParsed("TouOptionVigilanteGuessableKilling"));
        if (noValid) selected.Add(TouLocale.GetParsed("TouOptionVigilanteGuessableOutlier"));

        var names = selected
            .Distinct()
            .ToList();

        var newArray2 = new []
            { $"{title}: {string.Join(", ", names)}" };
        return newArray2;
    }
}