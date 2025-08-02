using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SpyOptions : AbstractOptionGroup<SpyRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Spy, "Spy");

    [ModdedEnumOption("Who Sees Dead Bodies On Admin", typeof(AdminDeadPlayers),
        ["Nobody", "Spy", "Everyone But Spy", "Everyone"])]
    public AdminDeadPlayers WhoSeesDead { get; set; } = AdminDeadPlayers.Nobody;

    [ModdedEnumOption("Allow Portable Admin Table For", typeof(PortableAdmin),
        ["Role", "Modifier", "Role & Modifier", "Disabled"])]
    public PortableAdmin HasPortableAdmin { get; set; } = PortableAdmin.Both;

    public ModdedToggleOption MoveWithMenu { get; } = new("Move While Using Portable Admin", true)
    {
        Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
    };

    public ModdedNumberOption StartingCharge { get; } =
        new("Starting Charge", 20f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
        };

    public ModdedNumberOption RoundCharge { get; } =
        new("Battery Charged Each Round", 15f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
        };

    public ModdedNumberOption TaskCharge { get; } =
        new("Battery Charged Per Task", 10f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
        };

    public ModdedNumberOption DisplayCooldown { get; } = new("Portable Admin Display Cooldown", 15f, 0f, 30f, 5f,
        MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is not PortableAdmin.None
    };

    public ModdedNumberOption DisplayDuration { get; } = new("Portable Admin Display Duration", 15f, 0f, 30f, 5f,
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