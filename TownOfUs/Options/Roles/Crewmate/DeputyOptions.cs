using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class DeputyOptions : AbstractRoleOptionGroup<DeputyRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Deputy", "Deputy");

    public ModdedToggleOption WarnKiller { get; set; } = new("TouOptionDeputyWarnKillerOnCampedKill", true);

    public ModdedEnumOption<DeputyReveal> RevealDeputyUponShot { get; set; } = new("TouOptionDeputyRevealDeputyUponShot", DeputyReveal.RevealPlayer, ["TouOptionDeputyEnumNoReveal", "TouOptionDeputyEnumAnnounceRole", "TouOptionDeputyEnumRevealPlayer"]);
}

public enum DeputyReveal
{
    NoReveal,
    AnnounceRole,
    RevealPlayer
}