using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SpyOptions : AbstractOptionGroup<SpyRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSpy", "Spy");

    [ModdedEnumOption("TouOptionSpyWhoSeesDeadBodiesOnAdmin", typeof(AdminDeadPlayers),
        ["TouOptionSpyDeadEnumNobody", "TouOptionSpyDeadEnumSpy", "TouOptionSpyDeadEnumEveryoneButSpy", "TouOptionSpyDeadEnumEveryone"])]
    public AdminDeadPlayers WhoSeesDead { get; set; } = AdminDeadPlayers.Nobody;

    [ModdedEnumOption("TouOptionSpyAllowPortableAdminTableFor", typeof(PortableAdmin),
        ["TouOptionSpyPortableEnumRole", "TouOptionSpyPortableEnumModifier", "TouOptionSpyPortableEnumBoth", "TouOptionSpyPortableEnumNone"])]
    public PortableAdmin HasPortableAdmin { get; set; } = PortableAdmin.Both;

    public ModdedToggleOption MoveWithMenu { get; } = new("TouOptionSpyMoveWhileUsingPortableAdmin", true)
    {
        Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
    };

    public ModdedNumberOption StartingCharge { get; } =
        new("TouOptionSpyStartingCharge", 20f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
        };

    public ModdedNumberOption RoundCharge { get; } =
        new("TouOptionSpyBatteryChargedEachRound", 15f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
        };

    public ModdedNumberOption TaskCharge { get; } =
        new("TouOptionSpyBatteryChargedPerTask", 10f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
        };

    public ModdedNumberOption DisplayCooldown { get; } = new("TouOptionSpyPortableAdminDisplayCooldown", 15f, 0f, 30f, 5f,
        MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
    };

    public ModdedNumberOption DisplayDuration { get; } = new("TouOptionSpyPortableAdminDisplayDuration", 15f, 0f, 30f, 5f,
        MiraNumberSuffixes.Seconds, zeroInfinity: true)
    {
        Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
    };
}

public enum PortableAdmin
{
    Role,
    Modifier,
    Both,
    None
}

public enum AdminDeadPlayers
{
    Nobody,
    Spy,
    EveryoneButSpy,
    Everyone
}