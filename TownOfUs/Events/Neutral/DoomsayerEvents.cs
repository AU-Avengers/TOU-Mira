using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class DoomsayerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        if (source.Data.Role is DoomsayerRole doom)
        {
            if (!source.AmOwner)
            {
                doom.NumberOfGuesses++;
            }
            if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
            {
                stats.CorrectAssassinKills++;
            }

            if (source.AmOwner && (int)OptionGroupSingleton<DoomsayerOptions>.Instance.DoomsayerGuessesToWin ==
                doom.NumberOfGuesses)
            {
                DoomsayerRole.RpcDoomsayerWin(source);
                GameHistory.RpcUpdateLocalDeathHandler(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer, "DiedToWinning",
                    HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }
        }
        else if (source.GetRoleWhenAlive() is DoomsayerRole &&
                 (MeetingHud.Instance || ExileController.Instance) &&
                 GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            // This should fix doomsayer's guesses appearing as regular postmortem kills
            stats.CorrectAssassinKills++;
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        if (OptionGroupSingleton<DoomsayerOptions>.Instance.DoomWin is DoomWinOptions.Leaves)
        {
            var doom = CustomRoleUtils.GetActiveRolesOfType<DoomsayerRole>()
                .FirstOrDefault(x => x.AllGuessesCorrect && !x.Player.HasDied());
            if (doom != null)
            {
                if (doom.Player.AmOwner)
                {
                    PlayerControl.LocalPlayer.DelayExile();
                    var notif1 = Helpers.CreateAndShowNotification(
                        $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.DoomsayerWonSelf").Replace("<role>", $"{TownOfUsColors.Doomsayer.ToTextColor()}{doom.GetRoleName()}</color>")}</b>",
                        Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Doomsayer);

                    notif1.AdjustNotification();
                }
                else
                {
                    string message;
                    LoadableAsset<Sprite> icon;

                    if (OptionGroupSingleton<DoomsayerOptions>.Instance.DoomAnonymizeWin)
                    {
                        message = MiraLocaleManager.Get("TouNeutAnonymousVictoryMessage");
                        icon = TouRoleIcons.Neutral;
                    }
                    else
                    {
                        message = $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.DoomsayerWonOther")
                            .Replace("<role>", $"{TownOfUsColors.Doomsayer.ToTextColor()}{doom.GetRoleName()}</color>")}</b>";
                        icon = TouRoleIcons.Doomsayer;
                    }

                    var notif1 = Helpers.CreateAndShowNotification(
                        message.Replace("<player>", doom.Player.Data.PlayerName),
                        Color.white, new Vector3(0f, 1f, -20f), spr: icon);

                    notif1.AdjustNotification();
                }
            }
        }

        // If the Doomsayer has used up all possible guesses (IsInFinalMeeting),
        // exile them so they no longer block the game from ending.
        var doomFinal = CustomRoleUtils.GetActiveRolesOfType<DoomsayerRole>()
            .FirstOrDefault(x => x.IsInFinalMeeting && !x.Player.HasDied());
        if (doomFinal != null)
        {
            if (GameHistory.PlayerStats.TryGetValue(doomFinal.Player.PlayerId, out var stats))
            {
                stats.DeathString = MiraLocaleManager.Get("DiedToWinning");
                stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
                stats.DiedThisRound = false;
                stats.PlayerState = StoredPlayerState.Dead;
                stats.LockDeathInfo = true;
            }

            doomFinal.Player.Exiled();
        }
    }
}