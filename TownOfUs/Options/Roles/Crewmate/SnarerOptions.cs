using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SnarerOptions : AbstractOptionGroup<SnarerRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSnarer", "Snarer");

    [ModdedNumberOption("TouOptionSnarerSnareCooldown", 1f, 60f, 1f, MiraNumberSuffixes.Seconds)]
    public float SnareCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionSnarerSnareDuration", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float SnareDuration { get; set; } = 4.5f;

    [ModdedNumberOption("TouOptionSnarerArrowDuration", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ArrowDuration { get; set; } = 5f;

    [ModdedNumberOption("TouOptionSnarerMaxSnares", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxSnares { get; set; } = 4f;

    [ModdedToggleOption("TouOptionSnarerSnaresRemovedAfterRound")]
    public bool SnaresRemoveOnNewRound { get; set; } = false;

    [ModdedNumberOption("TouOptionSnarerTasksUntilMoreSnares", 1f, 10f, 1f, MiraNumberSuffixes.None, "0")]
    public float TasksUntilMoreSnares { get; set; } = 2f;
}