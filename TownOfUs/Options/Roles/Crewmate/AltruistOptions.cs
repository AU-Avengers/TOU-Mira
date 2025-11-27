using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class AltruistOptions : AbstractOptionGroup<AltruistRole>
{
    public override string GroupName => TouLocale.Get("TouRoleAltruist", "Altruist");

    public ModdedEnumOption ReviveMode { get; } =
        new("TouOptionAltruistReviveType", (int)ReviveType.GroupSacrifice, typeof(ReviveType),
            ["TouOptionAltruistReviveEnumSacrifice", "TouOptionAltruistReviveEnumGroupSacrifice", "TouOptionAltruistReviveEnumGroupRevive"]);

    public ModdedNumberOption ReviveRange { get; } =
        new("TouOptionAltruistReviveRange", 0.25f, 0.05f, 1f, 0.05f,
            MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode != (int)ReviveType.Sacrifice
        };

    public ModdedNumberOption ReviveDuration { get; } =
        new("TouOptionAltruistReviveDuration", 5f, 1f, 15f, 1f,
            MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxRevives { get; } =
        new("TouOptionAltruistMaxRevives", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedToggleOption FreezeDuringRevive { get; } =
        new("TouOptionAltruistFreezeDuringRevive", true);

    public ModdedToggleOption HideAtBeginningOfRevive { get; } =
        new("TouOptionAltruistHideAtBeginningOfRevive", false);

    public ModdedEnumOption KillersAlertedAtStart { get; } =
        new("TouOptionAltruistKillersAlertedAtStart", (int)InformedKillers.Nobody, typeof(InformedKillers),
            ["TouOptionAltruistKillerEnumNobody", "TouOptionAltruistKillerEnumNeutrals", "TouOptionAltruistKillerEnumImpostors", "TouOptionAltruistKillerEnumNeutralsAndImpostors"]);

    public ModdedEnumOption KillersAlertedAtEnd { get; } =
        new("TouOptionAltruistKillersAlertedAtEnd", (int)InformedKillers.NeutralsAndImpostors, typeof(InformedKillers),
            ["TouOptionAltruistKillerEnumNobody", "TouOptionAltruistKillerEnumNeutrals", "TouOptionAltruistKillerEnumImpostors", "TouOptionAltruistKillerEnumNeutralsAndImpostors"]);
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