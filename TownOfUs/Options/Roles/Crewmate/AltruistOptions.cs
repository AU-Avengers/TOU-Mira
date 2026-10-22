using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class AltruistOptions : AbstractRoleOptionGroup<AltruistRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Altruist", "Altruist");

    public ModdedEnumOption<ReviveType> ReviveMode { get; } =
        new("TouOptionAltruistReviveType", ReviveType.GroupSacrifice,
            ["TouOptionAltruistReviveEnumSacrifice", "TouOptionAltruistReviveEnumGroupSacrifice", "TouOptionAltruistReviveEnumGroupRevive"]);

    public ModdedNumberOption ReviveRange { get; } =
        new("TouOptionAltruistReviveRange", 0.25f, 0.05f, 1f, 0.05f,
            MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode != ReviveType.Sacrifice
        };

    public ModdedNumberOption ReviveDuration { get; } =
        new("TouOptionAltruistReviveDuration", 5f, 1f, 15f, 1f,
            MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxRevives { get; } =
        new("TouOptionAltruistMaxRevives", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive
        };
        
    public ModdedToggleOption KillOnStartRevive { get; } =
        new("TouOptionAltruistKillOnStartRevive", false);

    public ModdedToggleOption FreezeDuringRevive { get; } =
        new("TouOptionAltruistFreezeDuringRevive", true)
        {
            Visible = () => !OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive
        };

    public ModdedToggleOption HideAtBeginningOfRevive { get; } =
        new("TouOptionAltruistHideAtBeginningOfRevive", false);

    public ModdedEnumOption<InformedKillers> KillersAlertedAtStart { get; } =
        new("TouOptionAltruistKillersAlertedAtStart", InformedKillers.Nobody,
            ["TouOptionAltruistKillerEnumNobody", "TouOptionAltruistKillerEnumNeutrals", "TouOptionAltruistKillerEnumImpostors", "TouOptionAltruistKillerEnumNeutralsAndImpostors"]);

    public ModdedEnumOption<InformedKillers> KillersAlertedAtEnd { get; } =
        new("TouOptionAltruistKillersAlertedAtEnd", InformedKillers.NeutralsAndImpostors,
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