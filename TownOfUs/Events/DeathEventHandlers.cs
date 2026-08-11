using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Events;

public static class DeathEventHandlers
{
    [RegisterEvent(-1)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            HudManagerHelper.Instance.CurrentRound = 1;
            Warning("Game Has Started");
        }
        else
        {
            ++HudManagerHelper.Instance.CurrentRound;
            foreach (var stats in GameHistory.PlayerStats.Values)
            {
                stats.DiedThisRound = false;
            }
            Warning($"New Round Started: {HudManagerHelper.Instance.CurrentRound}");
        }
    }

    [RegisterEvent(1000)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        var victim = @event.Player;
        var stats = GameHistory.PlayerStats[victim.PlayerId];
        if (stats.PlayerState is StoredPlayerState.Alive or StoredPlayerState.Revived)
        {
            var cod = "Disconnected";
            stats.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            switch (@event.DeathReason)
            {
                case DeathReason.Exile:
                    cod = "Ejection";
                    stats.DiedThisRound = false;
                    break;
                case DeathReason.Kill:
                    cod = "Killer";
                    break;
            }

            stats.DeathString = TouLocale.Get($"DiedTo{cod}");
            stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
            stats.PlayerState = StoredPlayerState.Dead;
        }
    }

    [RegisterEvent(10000)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }
        GameHistory.UpdatePlayerDeathData(exiled, TouLocale.Get("DiedToEjection"), 0, HudManagerHelper.Instance.CurrentRound,
            DeathHandlerOverride.SetFalse, playerState: StoredPlayerState.Dead);
    }

    [RegisterEvent(500)]
    public static void PlayerReviveEventHandler(PlayerReviveEvent reviveEvent)
    {
        var stats = GameHistory.PlayerStats[reviveEvent.Player.PlayerId];

        stats.PlayerState = StoredPlayerState.Revived;
        stats.DeathString = TouLocale.Get("Revived");
        stats.KilledBy = "";

        // Sync physics body position to match transform position after revive
        // This prevents wall-walking bugs that can occur when players are revived
        var player = reviveEvent.Player;
        if (player != null && player.MyPhysics?.body != null)
        {
            var pos = (Vector2)player.transform.position;
            player.MyPhysics.body.position = pos;
            Physics2D.SyncTransforms();
        }
    }

    [RegisterEvent(500)]
    public static void AfterMurderEventHandler(AfterMurderEvent murderEvent)
    {
        var source = murderEvent.Source;
        var target = murderEvent.Target;
        var stats = GameHistory.PlayerStats[target.PlayerId];

            if (stats.LockDeathInfo)
            {
                return;
            }

            stats.PlayerState = StoredPlayerState.Dead;
            if (target == source)
            {
                var role = target.GetRoleWhenAlive();
                var text = TouLocale.Get("DiedToSuicide");

                if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS" &&
                    !TouLocale.Get($"DiedToSuicide{touRole.LocaleKey}").Contains("STRMISS"))
                {
                    text = TouLocale.Get($"DiedToSuicide{touRole.LocaleKey}");
                }

                stats.DeathString = text;
                stats.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
                stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
                stats.LockDeathInfo = true;
            }
            else
            {
                var role = source.GetRoleWhenAlive();
                var cod = "Killer";
            
                var roleToCheck = role is MirrorcasterRole mirror ? mirror.ContainedRole ?? mirror : role;
                var localeKey = roleToCheck.GetRoleLocaleKey();
                if (localeKey != "KEY_MISS" &&
                    !TouLocale.Get($"DiedTo{localeKey}").Contains("STRMISS"))
                {
                    cod = localeKey;
                }

                if (source.Data.Role is IGhostRole && source.Data.Role is ITownOfUsRole touRole)
                {
                    cod = touRole.LocaleKey;
                }

                stats.DeathString = TouLocale.Get($"DiedTo{cod}");
                stats.KilledBy =
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName);
                stats.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
                stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
            }
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        if (!MeetingHud.Instance)
        {
            return;
        }

        var player = @event.ClientData.Character;

        if (!player)
        {
            return;
        }

        var pva = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);

        if (!pva)
        {
            return;
        }

        pva.AmDead = true;
        pva.Overlay.gameObject.SetActive(true);
        pva.Overlay.color = Color.white;
        pva.XMark.gameObject.SetActive(false);
        pva.XMark.transform.localScale = Vector3.one;

        MeetingMenu.Instances.Do(x => x.HideSingle(player.PlayerId));
    }
}