using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(GameData))]
public static class SummaryDisconnectFixPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        var playerRoleString = new StringBuilder();
        var playerRoleStringShort = new StringBuilder();

        var summaryTitle = new StringBuilder();
        var summaryRoleInfo = new StringBuilder();
        var summaryStats = new StringBuilder();
        var summaryCod = new StringBuilder();
        if (player.Data.Role is SpectatorRole)
        {
            EndGamePatches.EndGameData.DisconnectedPlayerRecords.Add(new EndGamePatches.EndGameData.PlayerRecord
            {
                ChatSummaryTitle = $"{player.Data.PlayerName} - {TouLocale.Get("TouRoleSpectator")}",
                ChatSummaryRoleInfo = string.Empty,
                ChatSummaryStats = string.Empty,
                ChatSummaryCod = string.Empty,
                PlayerName = player.Data.PlayerName,
                RoleString = TouLocale.Get("TouRoleSpectator"),
                RoleStringShort = TouLocale.Get("TouRoleSpectator"),
                Winner = false,
                LastRole = (RoleTypes)RoleId.Get<SpectatorRole>(),
                Team = ModdedRoleTeams.Custom,
                PlayerId = player.PlayerId
            });
            return;
        }

        var latestRole = string.Empty;
        var changedAgain = false;

        foreach (var role in GameHistory.RoleHistory.Where(x => x.Key == player.PlayerId)
                     .Select(x => x.Value))
        {
            if (role.Role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost ||
                role.Role == (RoleTypes)RoleId.Get<NeutralGhostRole>())
            {
                continue;
            }

            var color = role.TeamColor;
            string roleName;

            if (!string.IsNullOrEmpty(role.GetRoleName().Trim()))
            {
                roleName = role.GetRoleName();
            }
            else
            {
                roleName = TranslationController.Instance.GetString(role.Player.IsImpostor()
                    ? StringNames.Impostor
                    : StringNames.Crewmate);
            }

            if (latestRole != string.Empty)
            {
                changedAgain = true;
            }

            latestRole = $"{color.ToTextColor()}{roleName}</color>";

            playerRoleString.Append(TownOfUsPlugin.Culture, $"{color.ToTextColor()}{roleName}</color> > ");
        }

        if (playerRoleString.Length > 3)
        {
            playerRoleString = playerRoleString.Remove(playerRoleString.Length - 3, 3);
        }

        if (changedAgain)
        {
            summaryRoleInfo.Append(playerRoleString);
        }

        var lastRole = GameHistory.AllRoles.FirstOrDefault(x => x.Player.PlayerId == player.PlayerId);
        var playerRoleType = lastRole!.Role;
        var playerTeam = ModdedRoleTeams.Crewmate;

        if (lastRole is ITownOfUsRole touRole)
        {
            playerTeam = touRole.Team;
        }
        else if (lastRole.IsImpostor)
        {
            playerTeam = ModdedRoleTeams.Impostor;
        }

        var modifiers = player.GetModifiers<GameModifier>()
            .Where(x => x is TouGameModifier || x is UniversalGameModifier);
        var modifierCount = modifiers.Count();
        var modifierNames = modifiers.Select(modifier => modifier.ModifierName);
        if (modifierCount != 0)
        {
            playerRoleString.Append(TownOfUsPlugin.Culture, $" (");
        }

        foreach (var modifierName in modifierNames)
        {
            var modColor = MiscUtils.GetRoleColour(modifierName.Replace(" ", string.Empty));
            if (modColor == TownOfUsColors.Impostor)
            {
                modColor = MiscUtils.GetModifierColour(
                    modifiers.FirstOrDefault(x => x.ModifierName == modifierName)!);
            }

            modifierCount--;
            if (modifierCount == 0)
            {
                playerRoleString.Append(TownOfUsPlugin.Culture, $"{modColor.ToTextColor()}{modifierName}</color>)");
            }
            else
            {
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $"{modColor.ToTextColor()}{modifierName}</color>, ");
            }
        }

        var modifierHolder = new StringBuilder();
        var modifiersAlt = player.GetModifiers<GameModifier>()
            .Where(x => x is TouGameModifier || x is UniversalGameModifier || x is AllianceGameModifier);
        var modifierCountAlt = modifiersAlt.Count();
        var modifierNamesAlt = modifiersAlt.Select(modifier => modifier.ModifierName);
        if (modifierCountAlt != 0)
        {
            modifierHolder.Append(TownOfUsPlugin.Culture, $" (");
        }

        foreach (var modifierName in modifierNamesAlt)
        {
            var modColor = MiscUtils.GetRoleColour(modifierName.Replace(" ", string.Empty));
            if (modColor == TownOfUsColors.Impostor)
            {
                modColor = MiscUtils.GetModifierColour(
                    modifiersAlt.FirstOrDefault(x => x.ModifierName == modifierName)!);
            }

            modifierCountAlt--;
            if (modifierCountAlt == 0)
            {
                modifierHolder.Append(TownOfUsPlugin.Culture, $"{modColor.ToTextColor()}{modifierName}</color>)");
            }
            else
            {
                modifierHolder.Append(TownOfUsPlugin.Culture,
                    $"{modColor.ToTextColor()}{modifierName}</color>, ");
            }
        }

        if (player.IsRole<SpectreRole>() || playerTeam == ModdedRoleTeams.Crewmate)
        {
            var taskInfo = player.TaskInfo();
            playerRoleString.Append(TownOfUsPlugin.Culture,
                $" {taskInfo}");
            summaryStats.Append(TownOfUsPlugin.Culture,
                $" | {TouLocale.GetParsed("StatsTaskCount").Replace("<count>", taskInfo.Replace("(", "").Replace(")", ""))}");
        }

        var killedPlayers = GameHistory.KilledPlayers.Count(x =>
            x.KillerId == player.PlayerId && x.VictimId != player.PlayerId);

        if (GameHistory.PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            var basicKillCount = killedPlayers - stats.CorrectAssassinKills - stats.IncorrectKills -
                                 stats.IncorrectAssassinKills - stats.CorrectKills;
            if (stats.CorrectKills > 0)
            {
                summaryStats.Append(TownOfUsPlugin.Culture,
                    $" | {Color.green.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{stats.CorrectKills}")}</color>");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {Color.green.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{stats.CorrectKills}")}</color>");
            }
            else if (basicKillCount > 0 && !player.IsCrewmate())
            {
                summaryStats.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{basicKillCount}")}</color>");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{basicKillCount}")}</color>");
            }

            if (stats.IncorrectKills > 0)
            {
                summaryStats.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsBadKillCount").Replace("<count>", $"{stats.IncorrectKills}")}</color>");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsBadKillCount").Replace("<count>", $"{stats.IncorrectKills}")}</color>");
            }

            if (stats.CorrectAssassinKills > 0)
            {
                summaryStats.Append(TownOfUsPlugin.Culture,
                    $" | {Color.green.ToTextColor()}{TouLocale.GetParsed("StatsGoodGuessCount").Replace("<count>", $"{stats.CorrectAssassinKills}")}</color>");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {Color.green.ToTextColor()}{TouLocale.GetParsed("StatsGoodGuessCount").Replace("<count>", $"{stats.CorrectAssassinKills}")}</color>");
            }

            /*if (stats.IncorrectAssassinKills > 0)
            {
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsBadGuessCount").Replace("<count>", $"{stats.IncorrectAssassinKills}")}</color>");
            }*/
        }
        else if (killedPlayers > 0 && !player.IsCrewmate() && !player.Is(RoleAlignment.NeutralEvil))
        {
            summaryStats.Append(TownOfUsPlugin.Culture,
                $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{killedPlayers}")}</color>");
            playerRoleString.Append(TownOfUsPlugin.Culture,
                $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{killedPlayers}")}</color>");
        }

        playerRoleStringShort.Append(playerRoleString);

        if (player.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
        {
            playerRoleString.Append(TownOfUsPlugin.Culture,
                $" | {Color.yellow.ToTextColor()}{deathHandler.CauseOfDeath}</color>");
            playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                $" | {Color.yellow.ToTextColor()}{deathHandler.CauseOfDeath}</color>");
            summaryCod.Append(TownOfUsPlugin.Culture,
                $"{Color.yellow.ToTextColor()}{deathHandler.CauseOfDeath}</color>");
            if (deathHandler.KilledBy != string.Empty)
            {
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" {deathHandler.KilledBy}");
                summaryCod.Append(TownOfUsPlugin.Culture,
                    $" {deathHandler.KilledBy}");
            }

            playerRoleString.Append(TownOfUsPlugin.Culture,
                $" ({TouLocale.GetParsed("RoundOfDeath").Replace("<count>", $"{deathHandler.RoundOfDeath}")})");

            playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                $" ({TouLocale.GetParsed("RoundOfDeath").Replace("<count>", $"{deathHandler.RoundOfDeath}")})");

            summaryCod.Append(TownOfUsPlugin.Culture,
                $" ({TouLocale.GetParsed("RoundOfDeathLong").Replace("<count>", $"{deathHandler.RoundOfDeath}")})");
        }
        else
        {
            playerRoleString.Append(TownOfUsPlugin.Culture,
                $" | {Color.yellow.ToTextColor()}{TouLocale.Get("DiedToDisconnect")}</color> ({TouLocale.GetParsed("RoundOfDeath").Replace("<count>", $"{DeathEventHandlers.CurrentRound}")})");
            playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                $" | {Color.yellow.ToTextColor()}{TouLocale.Get("DiedToDisconnect")}</color> ({TouLocale.GetParsed("RoundOfDeath").Replace("<count>", $"{DeathEventHandlers.CurrentRound}")})");
            summaryCod.Append(TownOfUsPlugin.Culture,
                $"{Color.yellow.ToTextColor()}{TouLocale.Get("DiedToDisconnect")}</color> ({TouLocale.GetParsed("RoundOfDeathLong").Replace("<count>", $"{DeathEventHandlers.CurrentRound}")})");
        }

        var playerName = new StringBuilder();
        var playerWinner = false;

        if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == player.Data.PlayerName))
        {
            playerName.Append(TownOfUsPlugin.Culture, $"<color=#EFBF04>{player.Data.PlayerName}</color>");
            playerWinner = true;
        }
        else
        {
            playerName.Append(player.Data.PlayerName);
        }

        summaryTitle.Append(TownOfUsPlugin.Culture,
            $"{playerName.ToString()} - {latestRole}{modifierHolder.ToString()}");

        var alliance = player.GetModifiers<AllianceGameModifier>().FirstOrDefault();
        if (alliance != null)
        {
            var modColor = MiscUtils.GetModifierColour(alliance);

            playerName.Append(TownOfUsPlugin.Culture,
                $" <b>{modColor.ToTextColor()}<size=60%>{alliance.Symbol}</size></color></b>");
        }

        if (summaryStats.Length > 3)
        {
            summaryStats = summaryStats.Remove(0, 3);
        }

        EndGamePatches.EndGameData.DisconnectedPlayerRecords.Add(new EndGamePatches.EndGameData.PlayerRecord
        {
            ChatSummaryTitle = summaryTitle.ToString(),
            ChatSummaryRoleInfo = summaryRoleInfo.ToString(),
            ChatSummaryStats = summaryStats.ToString(),
            ChatSummaryCod = summaryCod.ToString(),
            PlayerName = playerName.ToString(),
            RoleString = playerRoleString.ToString(),
            RoleStringShort = playerRoleStringShort.ToString(),
            Winner = playerWinner,
            LastRole = playerRoleType,
            Team = playerTeam,
            PlayerId = player.PlayerId
        });
    }
}
