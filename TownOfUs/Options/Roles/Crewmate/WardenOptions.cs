using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class WardenOptions : AbstractOptionGroup<WardenRole>
{
    public override string GroupName => TouLocale.Get("TouRoleWarden", "Warden");

    [ModdedEnumOption("TouOptionWardenShowFortifyPlayer", typeof(FortifyOptions),
        ["TouOptionWardenFortEnumSelf", "TouOptionWardenFortEnumWarden", "TouOptionWardenFortEnumSelfAndWarden", "TouOptionWardenFortEnumEveryone"])]
    public FortifyOptions ShowFortified { get; set; } = FortifyOptions.SelfAndWarden;
}

public enum FortifyOptions
{
    Self,
    Warden,
    SelfAndWarden,
    Everyone
}