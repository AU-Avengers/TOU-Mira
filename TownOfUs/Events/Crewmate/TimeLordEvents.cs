using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class TimeLordEvents
{
    private static int ActiveRewindTaskCount;
    private static uint LastRewindUseTaskId = uint.MaxValue;

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return;
        }

        // Always reset to clear any stale position data from previous games/disconnects
        TimeLordRewindSystem.Reset();

        ActiveRewindTaskCount = 0;
        LastRewindUseTaskId = uint.MaxValue;
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            TimeLordRewindSystem.ClearHostTaskHistory();
        }
        if (PlayerControl.LocalPlayer?.Data?.Role is not TimeLordRole)
        {
            return;
        }

        var btn = CustomButtonSingleton<TimeLordRewindButton>.Instance;
        btn.SetUses((int)OptionGroupSingleton<TimeLordOptions>.Instance.MaxUses.Value);
        if (!btn.LimitedUses)
        {
            btn.Button?.usesRemainingText.gameObject.SetActive(false);
            btn.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            btn.Button?.usesRemainingText.gameObject.SetActive(true);
            btn.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Task != null && @event.Player != null)
        {
            TimeLordEventHandlers.RecordTaskComplete(@event.Player, @event.Task);
        }

        if (AmongUsClient.Instance != null &&
            AmongUsClient.Instance.AmHost &&
            @event.Task != null &&
            @event.Player != null &&
            OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind &&
            TimeLordRewindSystem.MatchHasTimeLord())
        {
            TimeLordRewindSystem.RecordHostTaskCompletion(@event.Player, @event.Task);
        }

        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not TimeLordRole)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastRewindUseTaskId)
        {
            ++ActiveRewindTaskCount;
            LastRewindUseTaskId = @event.Task.Id;
        }

        var opt = OptionGroupSingleton<TimeLordOptions>.Instance;
        var btn = CustomButtonSingleton<TimeLordRewindButton>.Instance;
        if (btn.LimitedUses && opt.UsesPerTasks.Value != 0 && opt.UsesPerTasks.Value <= ActiveRewindTaskCount)
        {
            ++btn.UsesLeft;
            btn.SetUses(btn.UsesLeft);
            ActiveRewindTaskCount = 0;
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (OptionGroupSingleton<TimeLordOptions>.Instance.CanUseVitals)
        {
            return;
        }

        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role is not TimeLordRole)
        {
            return;
        }

        var console = @event.Usable.TryCast<SystemConsole>();

        if (console == null)
        {
            return;
        }

        if (console.MinigamePrefab.TryCast<VitalsMinigame>())
        {
            @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        TimeLordRewindSystem.CancelRewindForMeeting();
    }
}