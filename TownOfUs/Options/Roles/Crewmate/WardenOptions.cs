using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class WardenOptions : AbstractOptionGroup<WardenRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Warden, "Warden");

    [ModdedEnumOption("Show Fortify Player", typeof(FortifyOptions), ["Fortified", "Warden", "Fortified + Warden", "Everyone"])]
    public FortifyOptions ShowFortified { get; set; } = FortifyOptions.SelfAndWarden;
}

public enum FortifyOptions
{
    Self,
    Warden,
    SelfAndWarden,
    Everyone
}