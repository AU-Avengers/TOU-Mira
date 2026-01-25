using MiraAPI.Events;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Events;

/// <summary>
/// Best-effort meeting UI resync when a revive happens during a meeting.
/// This is critical for Time Lord rewinds that race with a button/report:
/// some clients can receive the revive RPC after <see cref="MeetingHud"/> is created.
/// </summary>
public static class ReviveMeetingEventHandlers
{
    [RegisterEvent]
    public static void PlayerReviveEventHandler(PlayerReviveEvent @event)
    {
        var meeting = MeetingHud.Instance;
        if (!meeting)
        {
            return;
        }

        var player = @event.Player;
        if (!player || player.Data == null)
        {
            return;
        }

        if (player.Data.IsDead)
        {
            return;
        }

        var states = meeting.playerStates;
        if (states == null)
        {
            return;
        }

        var voteArea = states.FirstOrDefault(x => x && x.TargetPlayerId == player.PlayerId);
        if (voteArea == null)
        {
            return;
        }

        // Undo the "killed in meeting" visual state.
        voteArea.AmDead = false;
        try { voteArea.Overlay.gameObject.SetActive(false); } catch { /* ignored */ }
        try { voteArea.XMark.gameObject.SetActive(false); } catch { /* ignored */ }
        try { voteArea.PlayerIcon.gameObject.SetActive(true); } catch { /* ignored */ }

        meeting.SetDirtyBit(1U);
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            meeting.CheckForEndVoting();
        }
    }
}