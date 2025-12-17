using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SheriffOptions : AbstractOptionGroup<SheriffRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSheriff", "Sheriff");

    [ModdedNumberOption("TouOptionSheriffKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionSheriffCanSelfReport")]
    public bool SheriffBodyReport { get; set; } = false;

    [ModdedToggleOption("TouOptionSheriffAllowShootinginFirstRound")]
    public bool FirstRoundUse { get; set; } = false;

    [ModdedToggleOption("TouOptionSheriffCanShootNeutralBenignRoles")]
    public bool ShootNeutralBenign { get; set; } = false;

    [ModdedToggleOption("TouOptionSheriffCanShootNeutralEvilRoles")]
    public bool ShootNeutralEvil { get; set; } = true;

    [ModdedToggleOption("TouOptionSheriffCanShootNeutralKillingRoles")]
    public bool ShootNeutralKiller { get; set; } = true;

    [ModdedToggleOption("TouOptionSheriffCanShootNeutralOutlierRoles")]
    public bool ShootNeutralOutlier { get; set; } = true;

    [ModdedEnumOption("TouOptionSheriffMisfireKills", typeof(MisfireOptions), ["TouOptionSheriffKillEnumSheriff", "TouOptionSheriffKillEnumTarget", "TouOptionSheriffKillEnumBoth", "TouOptionSheriffKillEnumNobody"])]
    public MisfireOptions MisfireType { get; set; } = MisfireOptions.Sheriff;
}

public enum MisfireOptions
{
    Sheriff,
    Target,
    Both,
    Nobody
}