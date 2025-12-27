using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SheriffOptions : AbstractOptionGroup<SheriffRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleSheriff", "Sheriff");

    [ModdedNumberOption("TouOptionSheriffKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionSheriffCanSelfReport")]
    public bool SheriffBodyReport { get; set; } = false;

    [ModdedToggleOption("TouOptionSheriffAllowShootinginFirstRound")]
    public bool FirstRoundUse { get; set; } = false;
    public ModdedToggleOption ShootNeutralBenign { get; set; } = new("TouOptionSheriffCanShootNeutralBenignRoles", false);
    public ModdedToggleOption ShootNeutralEvil { get; set; } = new("TouOptionSheriffCanShootNeutralEvilRoles", true);
    public ModdedToggleOption ShootNeutralKiller { get; set; } = new("TouOptionSheriffCanShootNeutralKillingRoles", true);
    public ModdedToggleOption ShootNeutralOutlier { get; set; } = new("TouOptionSheriffCanShootNeutralOutlierRoles", true);

    [ModdedEnumOption("TouOptionSheriffMisfireKills", typeof(MisfireOptions), ["TouOptionSheriffKillEnumSheriff", "TouOptionSheriffKillEnumTarget", "TouOptionSheriffKillEnumBoth", "TouOptionSheriffKillEnumNobody"])]
    public MisfireOptions MisfireType { get; set; } = MisfireOptions.Sheriff;

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            ShootNeutralBenign.StringName,
            ShootNeutralEvil.StringName,
            ShootNeutralKiller.StringName,
            ShootNeutralOutlier.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var title = TouLocale.GetParsed("TouOptionSheriffValidNeutralShots");
        var nbValid = ShootNeutralBenign.Value;
        var neValid = ShootNeutralEvil.Value;
        var nkValid = ShootNeutralKiller.Value;
        var noValid = ShootNeutralOutlier.Value;

        if (!nbValid && !neValid && !nkValid && !noValid)
        {
            var newArray = new []
                { $"{title}: {TouLocale.GetParsed("TouOptionSheriffNeutShootNone")}" };
            return newArray;
        }

        var selected = new List<string>();
        if (nbValid) selected.Add(TouLocale.GetParsed("TouOptionSheriffNeutShootBenign"));
        if (neValid) selected.Add(TouLocale.GetParsed("TouOptionSheriffNeutShootEvil"));
        if (nkValid) selected.Add(TouLocale.GetParsed("TouOptionSheriffNeutShootKilling"));
        if (noValid) selected.Add(TouLocale.GetParsed("TouOptionSheriffNeutShootOutlier"));

        var names = selected
            .Distinct()
            .ToList();

        var newArray2 = new []
            { $"{title}: {string.Join(", ", names)}" };
        return newArray2;
    }
}

public enum MisfireOptions
{
    Sheriff,
    Target,
    Both,
    Nobody
}