using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class OfficerOptions : AbstractOptionGroup<OfficerRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleOfficer", "Officer");

    public ModdedNumberOption ShootCooldown { get; set; } = new("TouOptionOfficerShootCooldown", 5f, 2.5f, 30f, 2.5f,
        MiraNumberSuffixes.Seconds);

    public ModdedNumberOption LoadCooldown { get; set; } = new("TouOptionOfficerLoadCooldown", 30f, 2.5f, 120f, 2.5f,
        MiraNumberSuffixes.Seconds);

    public ModdedToggleOption CanSelfReport { get; set; } = new("TouOptionOfficerCanSelfReport", false);

    public ModdedToggleOption FirstRoundShooting { get; set; } = new("TouOptionOfficerFirstRound", false);

    public ModdedToggleOption CanOnlyShootActiveKillers { get; set; } =
        new("TouOptionOfficerCanOnlyShootActiveKillers", true);

    public ModdedToggleOption CrewKillingAreInnocent { get; set; } =
        new("TouOptionOfficerCrewKillingAreInnocent", false)
        {
            Visible = () => OptionGroupSingleton<OfficerOptions>.Instance.CanOnlyShootActiveKillers.Value
        };

    public ModdedToggleOption NonKillingNeutralsAreInnocent { get; set; } =
        new("TouOptionOfficerNonKillingNeutralsAreInnocent", false)
        {
            Visible = () => !OptionGroupSingleton<OfficerOptions>.Instance.CanOnlyShootActiveKillers.Value
        };

    public ModdedNumberOption MaxBulletsTotal { get; set; } = new("TouOptionOfficerMaxBulletsTotal", 9f, 3f, 15f, 1f,
        MiraNumberSuffixes.None);

    public ModdedNumberOption MaxBulletsAtOnce { get; set; } = new("TouOptionOfficerMaxBulletsAtOnce", 3f, 2f, 9f, 1f,
        MiraNumberSuffixes.None);

    public ModdedNumberOption RoundsPunished { get; set; } = new("TouOptionOfficerRoundPunishment", 1f, 1f, 5f, 1f,
        MiraNumberSuffixes.None);

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            ShootCooldown.StringName,
            LoadCooldown.StringName,
            MaxBulletsTotal.StringName,
            MaxBulletsAtOnce.StringName
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var bullets = TouLocale.GetParsed("TouOptionOfficerBulletsSummary");
        var maxBullets = (int)MaxBulletsTotal.Value;
        var activeBullets = (int)MaxBulletsAtOnce.Value;
        var cooldowns = TouLocale.GetParsed("TouOptionOfficerCooldownSummary");
        var shootCd = ShootCooldown.Value;
        var loadCd = LoadCooldown.Value;

        var newArray2 = new []
        {
            cooldowns.Replace("<shoot>", shootCd.ToString(TownOfUsPlugin.Culture)).Replace("<load>", loadCd.ToString(TownOfUsPlugin.Culture)),
            bullets.Replace("<once>", activeBullets.ToString(TownOfUsPlugin.Culture)).Replace("<total>", maxBullets.ToString(TownOfUsPlugin.Culture))
        };
        return newArray2;
    }
}
