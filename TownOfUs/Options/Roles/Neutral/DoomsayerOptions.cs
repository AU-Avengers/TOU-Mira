using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class DoomsayerOptions : AbstractRoleOptionGroup<DoomsayerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Doomsayer", "Doomsayer");

    [ModdedNumberOption("TouOptionDoomsayerCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float ObserveCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionDoomsayerNecessaryGuessed", 2f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float DoomsayerGuessesToWin { get; set; } = 3f;

    [ModdedToggleOption("TouOptionDoomsayerGuessCrewInvestigative")]
    public bool DoomGuessInvest { get; set; } = false;

    [ModdedToggleOption("TouOptionDoomsayerGuessesAllAtOnce")]
    public bool DoomsayerGuessAllAtOnce { get; set; } = false;

    public ModdedToggleOption DoomsayerKillOnlyLast { get; set; } = new("TouOptionDoomsayerOnlyKillLast", false)
    {
        Visible = () => OptionGroupSingleton<DoomsayerOptions>.Instance.DoomsayerGuessAllAtOnce
    };

    [ModdedToggleOption("TouOptionDoomsayerCantObserve")]
    public bool CantObserve { get; set; } = false;

    [ModdedEnumOption("TouOptionDoomsayerWin", typeof(DoomWinOptions), ["TouOptionDoomsayerWinEnumEndsGame", "TouOptionDoomsayerWinEnumLeaves", "TouOptionDoomsayerWinEnumNothing"])]
    public DoomWinOptions DoomWin { get; set; } = DoomWinOptions.Leaves;

    public ModdedToggleOption DoomAnonymizeWin { get; set; } =
        new("TouOptionNeutAnonymousVictoryWin", false)
    {
        Visible = () => OptionGroupSingleton<DoomsayerOptions>.Instance.DoomWin is not DoomWinOptions.EndsGame
    };

    public ModdedToggleOption DoomContinuesGame { get; set; } = new("TouOptionDoomsayerContinuesGame", true);
}

public enum DoomWinOptions
{
    EndsGame,
    Leaves,
    Nothing
}