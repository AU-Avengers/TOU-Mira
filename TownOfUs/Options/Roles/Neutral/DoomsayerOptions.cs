using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class DoomsayerOptions : AbstractOptionGroup<DoomsayerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Doomsayer, "Doomsayer");

    [ModdedNumberOption("Observe Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float ObserveCooldown { get; set; } = 20f;

    [ModdedNumberOption("Number Of Guesses Needed To Win", 2f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float DoomsayerGuessesToWin { get; set; } = 3f;
    [ModdedToggleOption("Doomsayer Can Guess Crew Investigative Roles")]
    public bool DoomGuessInvest { get; set; } = false;

    [ModdedToggleOption("Doomsayer Guesses All Roles At Once")]
    public bool DoomsayerGuessAllAtOnce { get; set; } = false;

    public ModdedToggleOption DoomsayerKillOnlyLast { get; set; } = new("Kill Only The Last Victim", false)
    {
        Visible = () => OptionGroupSingleton<DoomsayerOptions>.Instance.DoomsayerGuessAllAtOnce
    };

    [ModdedToggleOption("Doomsayer Can't Observe")]
    public bool CantObserve { get; set; } = false;

    [ModdedEnumOption("Doomsayer Win", typeof(DoomWinOptions), ["Ends Game", "Leaves In Victory", "Nothing"])]
    public DoomWinOptions DoomWin { get; set; } = DoomWinOptions.Leaves;
}

public enum DoomWinOptions
{
    EndsGame,
    Leaves,
    Nothing
}