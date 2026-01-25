using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class OracleOptions : AbstractOptionGroup<OracleRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleOracle", "Oracle");

    [ModdedNumberOption("TouOptionOracleConfessCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float ConfessCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionOracleBlessCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlessCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionOracleRevealAccuracy", 0f, 100f, suffixType: MiraNumberSuffixes.Percent)]
    public float RevealAccuracyPercentage { get; set; } = 80f;

    public ModdedToggleOption ShowNeutralBenignAsEvil { get; set; } = new("TouOptionOracleNeutralBenignShowEvil", false);

    public ModdedToggleOption ShowNeutralEvilAsEvil { get; set; } = new("TouOptionOracleNeutralEvilShowEvil", false);

    public ModdedToggleOption ShowNeutralKillingAsEvil { get; set; } = new("TouOptionOracleNeutralKillingShowEvil", true);

    public ModdedToggleOption ShowNeutralOutlierAsEvil { get; set; } = new("TouOptionOracleNeutralOutlierShowEvil", true);
    
    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            ShowNeutralBenignAsEvil.StringName,
            ShowNeutralEvilAsEvil.StringName,
            ShowNeutralKillingAsEvil.StringName,
            ShowNeutralOutlierAsEvil.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var title = TouLocale.GetParsed("TouOptionOracleNeutralsThatShowEvil");
        var nbValid = ShowNeutralBenignAsEvil.Value;
        var neValid = ShowNeutralEvilAsEvil.Value;
        var nkValid = ShowNeutralKillingAsEvil.Value;
        var noValid = ShowNeutralOutlierAsEvil.Value;

        if (!nbValid && !neValid && !nkValid && !noValid)
        {
            var newArray = new []
                { $"{title}: {TouLocale.GetParsed("TouOptionOracleNeutBadNone")}" };
            return newArray;
        }

        var selected = new List<string>();
        if (nbValid) selected.Add(TouLocale.GetParsed("TouOptionOracleNeutBadBenign"));
        if (neValid) selected.Add(TouLocale.GetParsed("TouOptionOracleNeutBadEvil"));
        if (nkValid) selected.Add(TouLocale.GetParsed("TouOptionOracleNeutBadKilling"));
        if (noValid) selected.Add(TouLocale.GetParsed("TouOptionOracleNeutBadOutlier"));

        var names = selected
            .Distinct()
            .ToList();

        var newArray2 = new []
            { $"{title}: {string.Join(", ", names)}" };
        return newArray2;
    }
}