using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class HunterOptions : AbstractOptionGroup<HunterRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Hunter, "Hunter");

    [ModdedNumberOption("Hunter Kill Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HunterKillCooldown { get; set; } = 25f;

    [ModdedNumberOption("Hunter Stalk Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float HunterStalkCooldown { get; set; } = 20f;

    [ModdedNumberOption("Hunter Stalk Duration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HunterStalkDuration { get; set; } = 25f;

    [ModdedNumberOption("Max Stalk Uses", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float StalkUses { get; set; } = 5;

    [ModdedToggleOption("Get More Uses From Completing Tasks")]
    public bool TaskUses { get; set; } = true;

    [ModdedToggleOption("Hunter Kills Last Voter If Voted Out")]
    public bool RetributionOnVote { get; set; } = true;

    [ModdedToggleOption("Hunter Can Report Who They've Killed")]
    public bool HunterBodyReport { get; set; } = false;
}