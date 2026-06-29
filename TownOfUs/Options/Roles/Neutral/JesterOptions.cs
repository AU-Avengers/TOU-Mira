using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class JesterOptions : AbstractOptionGroup<JesterRole>
{
    public override string GroupName => TouLocale.Get("TouRoleJester", "Jester");

    [ModdedToggleOption("TouOptionJesterCanButton")] public bool CanButton { get; set; } = true;

    [ModdedToggleOption("TouOptionJesterCanVent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("TouOptionJesterCanPoke")]
    public bool CanPoke { get; set; } = true;

    public ModdedNumberOption PokeCooldown { get; } =
        new("TouOptionJesterPokeCooldown", 25f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")
    {
        Visible = () => OptionGroupSingleton<JesterOptions>.Instance.CanPoke
    };

    [ModdedToggleOption("TouOptionJesterImpVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("TouOptionJesterScatterEnabled")]
    public bool ScatterOn { get; set; } = true;

    [ModdedNumberOption("TouOptionJesterScatterTimer", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float ScatterTimer { get; set; } = 25f;

    [ModdedEnumOption("TouOptionJesterAfterWin", typeof(JestWinOptions), ["TouOptionJesterWinEnumEndsGame", "TouOptionJesterWinEnumHaunts", "TouOptionJesterWinEnumNothing"])]
    public JestWinOptions JestWin { get; set; } = JestWinOptions.EndsGame;

    public ModdedToggleOption JestAnnounceWin { get; set; } =
        new("TouOptionJesterNotifyWin", true)
    {
        Visible = () => OptionGroupSingleton<JesterOptions>.Instance.JestWin is not JestWinOptions.EndsGame
    };
}

public enum JestWinOptions
{
    EndsGame,
    Haunts,
    Nothing
}