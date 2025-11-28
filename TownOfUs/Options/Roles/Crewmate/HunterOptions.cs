using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class HunterOptions : AbstractOptionGroup<HunterRole>
{
    public override string GroupName => TouLocale.Get("TouRoleHunter", "Hunter");

    [ModdedNumberOption("TouOptionHunterKillCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HunterKillCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionHunterStalkCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float HunterStalkCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionHunterStalkDuration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HunterStalkDuration { get; set; } = 25f;

    [ModdedNumberOption("TouOptionHunterStalkUses", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float StalkUses { get; set; } = 5;

    [ModdedToggleOption("TouOptionHunterTaskUses")]
    public bool TaskUses { get; set; } = true;

    [ModdedToggleOption("TouOptionHunterRetributionOnVote")]
    public bool RetributionOnVote { get; set; } = true;

    [ModdedToggleOption("TouOptionHunterHunterBodyReport")]
    public bool HunterBodyReport { get; set; } = false;
}