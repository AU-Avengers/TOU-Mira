using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TrapperOptions : AbstractOptionGroup<TrapperRole>
{
    public override string GroupName => TouLocale.Get("TouRoleTrapper", "Trapper");

    [ModdedNumberOption("TouOptionTrapperTrapCooldown", 1f, 60f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrapCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionTrapperTrappeduration", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float Trappeduration { get; set; } = 4.5f;

    [ModdedNumberOption("TouOptionTrapperArrowDuration", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ArrowDuration { get; set; } = 5f;

    [ModdedNumberOption("TouOptionTrapperMaxTraps", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTraps { get; set; } = 4f;

    [ModdedNumberOption("TouOptionTrapperTrapRoundsLast", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float TrapRoundsLast { get; set; } = 0f;

    [ModdedToggleOption("TouOptionTrapperGetMoreFromTasks")]
    public bool GetMoreFromTasks { get; set; } = true;

    [ModdedNumberOption("TouOptionTrapperTasksUntilMoreTraps", 1f, 10f, 1f, MiraNumberSuffixes.None, "0")]
    public float TasksUntilMoreTraps { get; set; } = 2f;

    [ModdedEnumOption("TouOptionTrapperTrapTargets", typeof(VentTrapTargets),
        ["TouOptionTrapperTrapTargetsEnumImpostors", "TouOptionTrapperTrapTargetsEnumImpostorsAndNeutrals", "TouOptionTrapperTrapTargetsEnumAll"])]
    public VentTrapTargets TrapTargets { get; set; } = VentTrapTargets.ImpostorsAndNeutrals;
}

public enum VentTrapTargets
{
    Impostors,
    ImpostorsAndNeutrals,
    All
}