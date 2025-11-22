using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using TownOfUs.GameOver;
using TownOfUs.Modules;
using TownOfUs.Patches;
using TownOfUs.Roles;

namespace TownOfUs.Events;

public static class EndGameEvents
{
    public static int winType;

    [RegisterEvent(-100)]
    public static void OnGameEnd(GameEndEvent @event)
    {
        winType = 0;
        var reason = EndGameResult.CachedGameOverReason;
        var neutralWinner = CustomRoleUtils.GetActiveRolesOfTeam(ModdedRoleTeams.Custom)
            .Any(x => x is ITownOfUsRole role && role.WinConditionMet());

        if (neutralWinner)
        {
            return;
        }

        if (reason is GameOverReason.CrewmatesByVote or GameOverReason.CrewmatesByTask
            or GameOverReason.ImpostorDisconnect or GameOverReason.HideAndSeek_CrewmatesByTimer)
        {
            winType = 1;
            GameHistory.WinningFaction =
                $"<color=#{Palette.CrewmateBlue.ToHtmlStringRGBA()}>{TouLocale.Get("CrewmateWin")}</color>";
        }
        else if (reason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage
                 or GameOverReason.ImpostorsByVote or GameOverReason.CrewmateDisconnect or GameOverReason.HideAndSeek_ImpostorsByKills)
        {
            winType = 2;
            GameHistory.WinningFaction =
                $"<color=#{Palette.ImpostorRed.ToHtmlStringRGBA()}>{TouLocale.Get("ImpostorWin")}</color>";
        }

        if (reason == CustomGameOver.GameOverReason<DrawGameOver>())
        {
            winType = 0;
        }
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        EndGamePatches.BuildEndGameSummary(@event.EndGameManager);
    }
}