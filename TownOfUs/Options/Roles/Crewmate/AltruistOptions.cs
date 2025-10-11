using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class AltruistOptions : AbstractOptionGroup<AltruistRole>
{
    public override string GroupName => TouLocale.Get("TouRoleAltruist", "Altruist");

    public ModdedNumberOption ReviveDuration { get; } =
        new(TouLocale.Get("TouOptionAltruistReviveDuration", "Revive Duration"), 10f, 1f, 15f, 1f,
            MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ReviveRange { get; } =
        new(TouLocale.Get("TouOptionAltruistReviveRange", "Revive Range"), 0.25f, 0.05f, 1f, 0.05f,
            MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption MaxRevives { get; } =
        new(TouLocale.Get("TouOptionAltruistMaxRevives", "Revive Uses"), 2f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedToggleOption HideAtBeginningOfRevive { get; } =
        new(TouLocale.Get("TouOptionAltruistHideAtBeginningOfRevive", "Hide Bodies at Beginning Of Revive"), false);
}