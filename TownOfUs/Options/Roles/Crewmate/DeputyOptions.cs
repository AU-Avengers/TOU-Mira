using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class DeputyOptions : AbstractRoleOptionGroup<DeputyRole>
{
    public override string GroupName => TouLocale.Get("TouRoleDeputy", "Deputy");

    public ModdedToggleOption WarnKiller { get; set; } = new("TouOptionDeputyWarnKillerOnCampedKill", true);

    public ModdedEnumOption RevealDeputyUponShot { get; set; } = new("TouOptionDeputyRevealDeputyUponShot", (int)DeputyReveal.RevealPlayer, typeof(DeputyReveal), ["TouOptionDeputyEnumNoReveal", "TouOptionDeputyEnumAnnounceRole", "TouOptionDeputyEnumRevealPlayer"]);
}

public enum DeputyReveal
{
    NoReveal,
    AnnounceRole,
    RevealPlayer
}