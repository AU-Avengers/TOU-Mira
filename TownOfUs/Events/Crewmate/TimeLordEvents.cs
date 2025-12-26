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

        // Reset all local rewind/task history on every client at the start of every game.
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
        // Host-side task history for optional "undo tasks during rewind".
        if (AmongUsClient.Instance != null &&
            AmongUsClient.Instance.AmHost &&
            @event.Task != null &&
            OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind &&
            TimeLordRewindSystem.MatchHasTimeLord())
        {
            TimeLordRewindSystem.RecordHostTaskCompletion(@event.Player, @event.Task);
        }

        // Local: gain additional rewind uses based on tasks completed (Engineer-style).
        if (!@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data.Role is not TimeLordRole)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastRewindUseTaskId)
        {
            // Prevent farming uses by repeatedly completing the same task over and over via rewind.
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
        // Cancel rewind without snapping positions: the base game caches "return positions" at meeting start.
        // If we snap here, players can return to the wrong place when the meeting ends.
        TimeLordRewindSystem.CancelRewindForMeeting();
    }
}


