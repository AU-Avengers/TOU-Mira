using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GameTimerOptions : AbstractOptionGroup
{
    public override Func<bool> GroupVisible => () =>
        !(GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek
            or GameModes.SeekFools);
    public override string GroupName => "End Game Timer";
    public override uint GroupPriority => 3;

    [ModdedToggleOption("Game Timer")] 
    public bool GameTimerEnabled { get; set; } = false;

    public ModdedNumberOption PauseInMeetings { get; } =
        new("Pause Timer In Meetings", 5f, 1f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled
        };

    public ModdedEnumOption TimerEndOption { get; } =
        new("On Timer End", 1, typeof(GameTimerType), ["Impostor Win", "Game Draw"])
        {
            Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled
        };

    public ModdedNumberOption GameTimeLimit { get; } =
        new("Game Time Limit", 15f, 1f, 30f, 0.5f, MiraNumberSuffixes.None, "0.0m")
        {
            Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled
        };
}

public enum GameTimerType
{
    Impostors,
    GameDraw
}

public enum PauseInMeetingsType
{
    Below5Minutes,
    Below10Minutes,
    Always
}