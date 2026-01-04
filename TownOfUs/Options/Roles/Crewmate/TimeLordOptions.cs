using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TimeLordOptions : AbstractOptionGroup<TimeLordRole>
{
    public override string GroupName => TouLocale.Get("TouRoleTimeLord", "Time Lord");

    [ModdedNumberOption("TouOptionTimeLordRewindCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RewindCooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionTimeLordRewindHistory", 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float RewindHistorySeconds { get; set; } = 7.5f;

    public ModdedNumberOption MaxUses { get; } = new("TouOptionTimeLordMaxUses", 3f, 0f, 15f, 1f, "∞", "#", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption UsesPerTasks { get; } = new("TouOptionTimeLordUsesPerTasks", 3f, 0f, 15f, 1f, "Off", "#",
        MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<TimeLordOptions>.Instance.MaxUses.Value != 0f
    };

    [ModdedToggleOption("TouOptionTimeLordCanUseVitals")]
    public bool CanUseVitals { get; set; } = false;

    [ModdedToggleOption("TouOptionTimeLordReviveOnRewind")]
    public bool ReviveOnRewind { get; set; } = true;

    [ModdedToggleOption("TouOptionTimeLordUndoTasksOnRewind")]
    public bool UndoTasksOnRewind { get; set; } = true;

    [ModdedToggleOption("TouOptionTimeLordUncleanBodiesOnRewind")]
    public bool UncleanBodiesOnRewind { get; set; } = true;

    [ModdedToggleOption("TouOptionTimeLordNotifyOnRevive")]
    public bool NotifyOnRevive { get; set; } = false;
}