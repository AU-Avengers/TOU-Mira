using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class AltruistOptions : AbstractOptionGroup<AltruistRole>
{
    public override string GroupName => TouLocale.Get("TouRoleAltruist", "Altruist");

    public ModdedEnumOption ReviveMode { get; } =
        new("Revive Type", (int)ReviveType.GroupSacrifice, typeof(ReviveType),
            ["Sacrifice", "Group Sacrifice", "Group Revive"]);

    public ModdedNumberOption ReviveRange { get; } =
        new(TouLocale.Get("TouOptionAltruistReviveRange", "Revive Range"), 0.25f, 0.05f, 1f, 0.05f,
            MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode != (int)ReviveType.Sacrifice
        };

    public ModdedNumberOption ReviveDuration { get; } =
        new(TouLocale.Get("TouOptionAltruistReviveDuration", "Revive Duration"), 5f, 1f, 15f, 1f,
            MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxRevives { get; } =
        new(TouLocale.Get("TouOptionAltruistMaxRevives", "Revive Uses"), 2f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedToggleOption FreezeDuringRevive { get; } =
        new(TouLocale.Get("TouOptionAltruistFreezeDuringRevive", "Freeze Altruist During Revive"), true);

    public ModdedToggleOption HideAtBeginningOfRevive { get; } =
        new(TouLocale.Get("TouOptionAltruistHideAtBeginningOfRevive", "Hide Bodies at Beginning Of Revive"), false);

    public ModdedEnumOption KIllersAlertedAtStart { get; } =
        new("Killers Alerted Before Revive", (int)InformedKillers.Nobody, typeof(InformedKillers),
            ["Nobody", "Neutral Killers", "Impostors", "Neutrals and Impostors"]);

    public ModdedEnumOption KIllersAlertedAtEnd { get; } =
        new("Killers Alerted After Revive", (int)InformedKillers.NeutralsAndImpostors, typeof(InformedKillers),
            ["Nobody", "Neutral Killers", "Impostors", "Neutrals and Impostors"]);
}

public enum InformedKillers
{
    Nobody,
    Neutrals,
    Impostors,
    NeutralsAndImpostors,
}

public enum ReviveType
{
    Sacrifice,
    GroupSacrifice,
    GroupRevive,
}