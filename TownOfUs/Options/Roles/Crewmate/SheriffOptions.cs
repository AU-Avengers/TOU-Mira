using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SheriffOptions : AbstractRoleOptionGroup<SheriffRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => MiraLocaleManager.Get("TouRoleSheriff", "Sheriff");

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
        var title = MiraLocaleManager.Get("TouOptionSheriffValidNeutralShots");
        var nbValid = ShootNeutralBenign.Value;
        var neValid = ShootNeutralEvil.Value;
        var nkValid = ShootNeutralKiller.Value;
        var noValid = ShootNeutralOutlier.Value;

        if (!nbValid && !neValid && !nkValid && !noValid)
        {
            var newArray = new []
                { $"{title}: {MiraLocaleManager.Get("TouOptionSheriffNeutShootNone")}" };
            return newArray;
        }

        var selected = new List<string>();
        if (nbValid) selected.Add(MiraLocaleManager.Get("TouOptionSheriffNeutShootBenign"));
        if (neValid) selected.Add(MiraLocaleManager.Get("TouOptionSheriffNeutShootEvil"));
        if (nkValid) selected.Add(MiraLocaleManager.Get("TouOptionSheriffNeutShootKilling"));
        if (noValid) selected.Add(MiraLocaleManager.Get("TouOptionSheriffNeutShootOutlier"));

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