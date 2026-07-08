using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class DeputyOptions : AbstractOptionGroup<DeputyRole>
{
    public override string GroupName => TouLocale.Get("TouRoleDeputy", "Deputy");

    [ModdedToggleOption("TouOptionDeputyWarnKillerOnCampedKill")]
    public bool WarnKiller { get; set; } = false;
}