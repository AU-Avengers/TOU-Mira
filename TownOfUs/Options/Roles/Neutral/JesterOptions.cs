using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class JesterOptions : AbstractRoleOptionGroup<JesterRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleJester", "Jester");

    [ModdedToggleOption("TouOptionJesterCanButton")] public bool CanButton { get; set; } = true;
    public ModdedToggleOption CanVent { get; } =
        new("TouOptionJesterCanVent", true);

    public ModdedNumberOption VentCooldown { get; } =
        new("TouOptionJesterVentCooldown", 15f, 0f, 25f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")
    {
        Visible = () => OptionGroupSingleton<JesterOptions>.Instance.CanVent
    };

    public ModdedNumberOption VentDuration { get; } =
        new("TouOptionJesterVentDuration", 45f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0", zeroInfinity: true)
    {
        Visible = () => OptionGroupSingleton<JesterOptions>.Instance.CanVent
    };

    public ModdedToggleOption CanPoke { get; } =
        new("TouOptionJesterCanPoke", true);

    public ModdedNumberOption PokeCooldown { get; } =
        new("TouOptionJesterPokeCooldown", 25f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")
    {
        Visible = () => OptionGroupSingleton<JesterOptions>.Instance.CanPoke
    };

    [ModdedToggleOption("TouOptionJesterImpVision")]
    public bool ImpostorVision { get; set; } = true;

    public ModdedToggleOption ScatterOn { get; } =
        new("TouOptionJesterScatterEnabled", false);

    public ModdedNumberOption ScatterTimer { get; } =
        new("TouOptionJesterScatterTimer", 25f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")
        {
            Visible = () => OptionGroupSingleton<JesterOptions>.Instance.ScatterOn
        };

    [ModdedEnumOption("TouOptionJesterAfterWin", typeof(JestWinOptions), ["TouOptionJesterWinEnumEndsGame", "TouOptionJesterWinEnumHaunts", "TouOptionJesterWinEnumNothing"])]
    public JestWinOptions JestWin { get; set; } = JestWinOptions.EndsGame;

    public ModdedToggleOption JestAnnounceWin { get; set; } =
        new("TouOptionJesterNotifyWin", true)
    {
        Visible = () => OptionGroupSingleton<JesterOptions>.Instance.JestWin is not JestWinOptions.EndsGame
    };

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            ScatterOn.StringName,
            CanPoke.StringName,
            CanVent.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        return [];
    }
}

public enum JestWinOptions
{
    EndsGame,
    Haunts,
    Nothing
}