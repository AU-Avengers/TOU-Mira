using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class VampireOptions : AbstractOptionGroup<VampireRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleVampire", "Vampire");

    [ModdedNumberOption("TouOptionVampireBiteCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BiteCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionVampireMaxVamps", 2, 5, 1, MiraNumberSuffixes.None, "0")]
    public float MaxVampires { get; set; } = 2;

    [ModdedToggleOption("TouOptionVampireImpostorVision")]
    public bool HasVision { get; set; } = true;

    [ModdedToggleOption("TouOptionVampireNewVampsAssassinate")]
    public bool CanGuessAsNewVamp { get; set; } = true;
    public ModdedToggleOption ConvertNeutralBenign { get; set; } = new("TouOptionVampireConvertNeutralBenign", true);
    public ModdedToggleOption ConvertNeutralEvil { get; set; } = new("TouOptionVampireConvertNeutralEvil", true);
    public ModdedToggleOption ConvertNeutralOutlier { get; set; } = new("TouOptionVampireConvertNeutralOutlier", false);
    public ModdedToggleOption ConvertLovers { get; set; } = new("TouOptionVampireConvertLovers", false);

    [ModdedToggleOption("TouOptionVampireNewVampiresConvert")]
    public bool CanConvertAsNewVamp { get; set; } = true;

    [ModdedToggleOption("TouOptionVampireCanVent")]
    public bool CanVent { get; set; } = true;

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            ConvertNeutralBenign.StringName,
            ConvertNeutralEvil.StringName,
            ConvertNeutralOutlier.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var title = TouLocale.GetParsed("TouOptionVampireValidNeutralConversions");
        var nbValid = ConvertNeutralBenign.Value;
        var neValid = ConvertNeutralEvil.Value;
        var noValid = ConvertNeutralOutlier.Value;

        if (!nbValid && !neValid && !noValid)
        {
            var newArray = new []
                { $"{title}: {TouLocale.GetParsed("TouOptionVampireNeutConvertNone")}" };
            return newArray;
        }

        var selected = new List<string>();
        if (nbValid) selected.Add(TouLocale.GetParsed("TouOptionVampireNeutConvertBenign"));
        if (neValid) selected.Add(TouLocale.GetParsed("TouOptionVampireNeutConvertEvil"));
        if (noValid) selected.Add(TouLocale.GetParsed("TouOptionVampireNeutConvertOutlier"));

        var names = selected
            .Distinct()
            .ToList();

        var newArray2 = new []
            { $"{title}: {string.Join(", ", names)}" };
        return newArray2;
    }
}
