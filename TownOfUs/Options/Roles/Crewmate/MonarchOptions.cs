using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MonarchOptions : AbstractOptionGroup<MonarchRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Monarch, "Monarch");

    [ModdedNumberOption("Knight Cooldown", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KnightCooldown { get; set; } = 20f;

    [ModdedNumberOption("Maximum Knights", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxKnights { get; set; } = 2f;
    [ModdedNumberOption("Votes Per Knight", 1f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float VotesPerKnight { get; set; } = 1f;

    [ModdedToggleOption("Allow Knighted Self Votes")]
    public bool AllowSelfVotes { get; set; } = false;
    [ModdedNumberOption("Knight Delay", 1f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float KnightDelay { get; set; } = 3f;
    [ModdedToggleOption("Inform Monarch If a Knight Dies")]
    public bool InformWhenKnightDies { get; set; } = true;

    [ModdedToggleOption("Crew Knights Grant Kill Immunity")]
    public bool CrewKnightsGrantKillImmunity { get; set; } = true;

}