using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GameTimerOptions : AbstractOptionGroup<ClassicMode>
{
 public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.GameTimer");
    public override uint GroupPriority => 5;

    [ModdedToggleOption("TouOptionGameTimerEnabled")] 
    public bool GameTimerEnabled { get; set; } = false;

    public ModdedNumberOption PauseInMeetings { get; } =
        new("TouOptionGameTimerPauseInMeetings", 5f, 1f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled
        };

    public ModdedEnumOption TimerEndOption { get; } =
        new("TouOptionGameTimerEndOption", 1, typeof(GameTimerType),
            [
                "TouOptionGameTimerEndEnumImpostorWin",
                "TouOptionGameTimerEndEnumGameDraw"
            ])
        {
            Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled
        };

    public ModdedNumberOption GameTimeLimit { get; } =
        new("TouOptionGameTimerTimeLimit", 15f, 1f, 30f, 0.5f, MiraNumberSuffixes.None, "0.0m")
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