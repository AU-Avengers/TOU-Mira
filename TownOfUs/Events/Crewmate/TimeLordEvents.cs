using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Networking;
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
            var temporaryRevives = ModifierUtils.GetPlayersWithModifier<TimeLordTempReviveModifier>().ToList();
            var timeLord = ModifierUtils.GetActiveModifiers<TimeLordTempReviveModifier>().FirstOrDefault()?.TimeLord;
            if (temporaryRevives.Count > 0)
            {
                var players = new List<PlayerControl>();
                foreach (var temp in temporaryRevives)
                {
                    if (!temp.HasModifier<InvulnerabilityModifier>() && !temp.HasDied())
                    {
                        players.Add(temp);
                    }
                    temp.RemoveModifier<TimeLordTempReviveModifier>();
                }

                if (PlayerControl.LocalPlayer.IsHost())
                {
                    foreach (var player in players)
                    {
                        player.RpcSelfMurder(player, timeLord ?? player, true, true, false, false, false, false, "TempRevive");
                    }
                }
            }
            return;
        }

        // Always reset to clear any stale position data from previous games/disconnects
        TimeLordRewindSystem.Reset();

        ActiveRewindTaskCount = 0;
        LastRewindUseTaskId = uint.MaxValue;
        if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
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
        if (@event.Task && @event.Player)
        {
            TimeLordEventHandlers.RecordTaskComplete(@event.Player, @event.Task);
        }

        if (AmongUsClient.Instance &&
            AmongUsClient.Instance.AmHost &&
            @event.Task &&
            @event.Player &&
            OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind &&
            TimeLordRewindSystem.MatchHasTimeLord())
        {
            TimeLordRewindSystem.RecordHostTaskCompletion(@event.Player, @event.Task);
        }

        if (!@event.Player || !@event.Player.AmOwner || !@event.Player.Data)
        {
            return;
        }

        if (@event.Player.Data.Role is not TimeLordRole)
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

        if (!PlayerControl.LocalPlayer ||
            !PlayerControl.LocalPlayer.Data ||
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
    public static void StartMeetingEventHandler(StartMeetingEvent _)
    {
        TimeLordRewindSystem.CancelRewindForMeeting();
    }
}