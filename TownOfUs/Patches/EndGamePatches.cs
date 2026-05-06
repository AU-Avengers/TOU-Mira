using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System.Text;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using TMPro;
using TownOfUs.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class EndGamePatches
{
    public static void BuildEndGameData()
    {
        EndGameData.Clear();
        ContainedMeetingData.Clear();

        var playerRoleString = new StringBuilder();
        var playerRoleStringShort = new StringBuilder();

        var summaryTitle = new StringBuilder();
        var summaryRoleInfo = new StringBuilder();
        var summaryStats = new StringBuilder();
        var summaryCod = new StringBuilder();

        // Theres a better way of doing this e.g. switch statement or dictionary. But this works for now.
        // Oh god lmao
        foreach (var playerControl in PlayerControl.AllPlayerControls)
        {
            playerRoleString.Clear();
            playerRoleStringShort.Clear();
            summaryTitle.Clear();
            summaryRoleInfo.Clear();
            summaryStats.Clear();
            summaryCod.Clear();
            if (playerControl.Data.Role is SpectatorRole)
            {
                EndGameData.PlayerRecords.Add(new EndGameData.PlayerRecord
                {
                    ChatSummaryTitle = $"{playerControl.Data.PlayerName} - {TouLocale.Get("TouRoleSpectator")}",
                    ChatSummaryRoleInfo = string.Empty,
                    ChatSummaryStats = string.Empty,
                    ChatSummaryCod = string.Empty,
                    PlayerName = playerControl.Data.PlayerName,
                    RoleString = TouLocale.Get("TouRoleSpectator"),
                    RoleStringShort = TouLocale.Get("TouRoleSpectator"),
                    Winner = false,
                    LastRole = (RoleTypes)RoleId.Get<SpectatorRole>(),
                    Team = ModdedRoleTeams.Custom,
                    PlayerId = playerControl.PlayerId
                });
                continue;
            }

            var latestRole = string.Empty;
            var changedAgain = false;

            foreach (var role in GameHistory.RoleHistory.Where(x => x.Key == playerControl.PlayerId)
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

            var lastRole = GameHistory.AllRoles.FirstOrDefault(x => x.Player.PlayerId == playerControl.PlayerId);
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

            var modifiers = playerControl.GetModifiers<GameModifier>()
                .Where(x => x is TouGameModifier touMod && touMod.AppearsInSummary || x is UniversalGameModifier);
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
            var modifiersAlt = playerControl.GetModifiers<GameModifier>()
                .Where(x => x is TouGameModifier touMod && touMod.AppearsInSummary || x is UniversalGameModifier || x is AllianceGameModifier);
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

            if (playerControl.IsRole<SpectreRole>() || playerTeam == ModdedRoleTeams.Crewmate)
            {
                var taskInfo = playerControl.TaskInfo();
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" {taskInfo}");
                summaryStats.Append(TownOfUsPlugin.Culture, $" | {TouLocale.GetParsed("StatsTaskCount").Replace("<count>", taskInfo.Replace("(", "").Replace(")", ""))}");
            }

            var killedPlayers = GameHistory.KilledPlayers.Count(x =>
                x.KillerId == playerControl.PlayerId && x.VictimId != playerControl.PlayerId);

            if (GameHistory.PlayerStats.TryGetValue(playerControl.PlayerId, out var stats))
            {
                var basicKillCount = killedPlayers - stats.CorrectAssassinKills - stats.IncorrectKills - stats.IncorrectAssassinKills - stats.CorrectKills;
                if (stats.CorrectKills > 0)
                {
                    summaryStats.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{stats.CorrectKills}")}</color>");
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{stats.CorrectKills}")}</color>");
                }
                else if (basicKillCount > 0 && !playerControl.IsCrewmate())
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
            else if (killedPlayers > 0 && !playerControl.IsCrewmate() && !playerControl.Is(RoleAlignment.NeutralEvil))
            {
                summaryStats.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{killedPlayers}")}</color>");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{TouLocale.GetParsed("StatsKillCount").Replace("<count>", $"{killedPlayers}")}</color>");
            }

            playerRoleStringShort.Append(playerRoleString);

            if (playerControl.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
            {
                var hasExtendedCauseOfDeath = !string.IsNullOrEmpty(deathHandler.ExtendedCauseOfDeath);
                var causeOfDeath = hasExtendedCauseOfDeath
                    ? deathHandler.ExtendedCauseOfDeath
                    : deathHandler.CauseOfDeath;

                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}{causeOfDeath}</color>");
                playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}{deathHandler.CauseOfDeath}</color>");
                summaryCod.Append(TownOfUsPlugin.Culture,
                    $"{Color.yellow.ToTextColor()}{causeOfDeath}</color>");
                if (!hasExtendedCauseOfDeath && deathHandler.KilledBy != string.Empty)
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
                    $" | {Color.yellow.ToTextColor()}{TouLocale.Get("Alive")}</color>");
                playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}{TouLocale.Get("Alive")}</color>");
                summaryCod.Append(TownOfUsPlugin.Culture,
                    $"{Color.yellow.ToTextColor()}{TouLocale.Get("Alive")}</color>");
            }

            var playerName = new StringBuilder();
            var playerWinner = false;

            if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == playerControl.Data.PlayerName))
            {
                playerName.Append(TownOfUsPlugin.Culture, $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>");
                playerWinner = true;
            }
            else
            {
                playerName.Append(playerControl.Data.PlayerName);
            }
            summaryTitle.Append(TownOfUsPlugin.Culture, $"{playerName.ToString()} - {latestRole}{modifierHolder.ToString()}");

            var alliance = playerControl.GetModifiers<AllianceGameModifier>().FirstOrDefault();
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

            EndGameData.PlayerRecords.Add(new EndGameData.PlayerRecord
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
                PlayerId = playerControl.PlayerId
            });
        }

        foreach (var disconnected in EndGameData.DisconnectedPlayerRecords)
        {
            EndGameData.PlayerRecords.Add(disconnected);
        }
        EndGameData.PlayerRecords = EndGameData.PlayerRecords.OrderByDescending(x => x.Winner).ThenBy(x => x.LastRole).ToList();
        EndGameData.DisconnectedPlayerRecords.Clear();
    }

    public static void BuildEndGameSummary(EndGameManager instance)
    {
        var winText = instance.WinText;
        var exitBtn = instance.Navigation.ExitButton;

        var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
        var roleSummaryLeft = Object.Instantiate(winText.gameObject);
        roleSummaryLeft.transform.position = new Vector3(exitBtn.transform.position.x + 0.1f, position.y - 0.1f, -14f);
        roleSummaryLeft.transform.localScale = new Vector3(1f, 1f, 1f);
        roleSummaryLeft.gameObject.SetActive(false);

        var roleSummary = Object.Instantiate(winText.gameObject);
        roleSummary.transform.position = new Vector3(exitBtn.transform.position.x + 0.1f, position.y - 0.1f, -14f);
        roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

        var roleSummary2 = Object.Instantiate(winText.gameObject);
        roleSummary2.transform.position = new Vector3(exitBtn.transform.position.x + 0.1f, position.y - 0.1f, -14f);
        roleSummary2.transform.localScale = new Vector3(1f, 1f, 1f);

        winText.transform.position += Vector3.down * 0.8f;
        winText.text = $"\n{winText.text}";
        winText.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        var roleSummaryText1 = new StringBuilder();
        var roleSummaryText2 = new StringBuilder();
        var roleSummaryTextFull = new StringBuilder();
        var segmentedSummary = new StringBuilder();
        var basicSummary = new StringBuilder();
        var normalSummary = new StringBuilder();
        var summaryTxt = TouLocale.Get("EndGameSummary") + ":";
        roleSummaryText1.AppendLine(summaryTxt);
        roleSummaryTextFull.AppendLine(summaryTxt);
        var count = 0;
        foreach (var data in EndGameData.PlayerRecords)
        {
            var role = string.Join(" ", data.RoleString);
            var role2 = string.Join(" ", data.RoleStringShort);
            if (count % 2 == 0)
            {
                roleSummaryText2.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role2}");
            }
            else
            {
                roleSummaryText1.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role2}");
            }

            count++;
            roleSummaryTextFull.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role}");
            normalSummary.AppendLine(TownOfUsPlugin.Culture, $"<size=62%>{data.PlayerName} - {role}");
            basicSummary.AppendLine(TownOfUsPlugin.Culture, $"<size=62%>{data.PlayerName} - {role2}");

            segmentedSummary.AppendLine(TownOfUsPlugin.Culture, $"<size=70%>{data.ChatSummaryTitle}</size>");
            segmentedSummary.Append(TownOfUsPlugin.Culture, $"<size=62%>");
            if (!data.ChatSummaryRoleInfo.IsNullOrWhiteSpace())
            {
                segmentedSummary.AppendLine(TownOfUsPlugin.Culture, $"•{data.ChatSummaryRoleInfo}");
            }
            if (!data.ChatSummaryStats.IsNullOrWhiteSpace())
            {
                segmentedSummary.AppendLine(TownOfUsPlugin.Culture, $"•{data.ChatSummaryStats}");
            }
            segmentedSummary.AppendLine(TownOfUsPlugin.Culture, $"•{data.ChatSummaryCod}");
            segmentedSummary.Append(TownOfUsPlugin.Culture, $"</size>");
        }

        var roleSummaryTextMesh = roleSummary.GetComponent<TMP_Text>();
        roleSummaryTextMesh.alignment = TextAlignmentOptions.TopLeft;
        roleSummaryTextMesh.color = Color.white;
        roleSummaryTextMesh.fontSizeMin = 1.1f;
        roleSummaryTextMesh.fontSizeMax = 1.1f;
        roleSummaryTextMesh.fontSize = 1.1f;

        var roleSummaryTextMesh2 = roleSummary2.GetComponent<TMP_Text>();
        roleSummaryTextMesh2.alignment = TextAlignmentOptions.TopLeft;
        roleSummaryTextMesh2.color = Color.white;
        roleSummaryTextMesh2.fontSizeMin = 1.1f;
        roleSummaryTextMesh2.fontSizeMax = 1.1f;
        roleSummaryTextMesh2.fontSize = 1.1f;

        var roleSummaryTextMeshLeft = roleSummaryLeft.GetComponent<TMP_Text>();
        roleSummaryTextMeshLeft.alignment = TextAlignmentOptions.TopLeft;
        roleSummaryTextMeshLeft.color = Color.white;
        roleSummaryTextMeshLeft.fontSizeMin = 1.1f;
        roleSummaryTextMeshLeft.fontSizeMax = 1.1f;
        roleSummaryTextMeshLeft.fontSize = 1.1f;
        /* var controllerHandler = Object.FindObjectOfType<ControllerDisconnectHandler>();
        if (controllerHandler != null)
        {
            roleSummaryTextMesh.font = controllerHandler.ContinueText.GetComponent<TMP_Text>().font;
            roleSummaryTextMesh.fontStyle = FontStyles.Bold;
        } */

        var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
        roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
        roleSummaryTextMesh.text = roleSummaryText1.ToString();

        var roleSummaryTextMeshRectTransform2 = roleSummaryTextMesh2.GetComponent<RectTransform>();
        roleSummaryTextMeshRectTransform2.anchoredPosition = new Vector2(position.x + 8.8f, position.y - 0.1f);
        roleSummaryTextMesh2.text = roleSummaryText2.ToString();

        var roleSummaryTextMeshRectTransformLeft = roleSummaryTextMeshLeft.GetComponent<RectTransform>();
        roleSummaryTextMeshRectTransformLeft.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
        roleSummaryTextMeshLeft.text = roleSummaryTextFull.ToString();

        GameHistory.EndGameSummarySimple = basicSummary.ToString();
        GameHistory.EndGameSummary = normalSummary.ToString();
        GameHistory.EndGameSummaryAdvanced = segmentedSummary.ToString();

        var GameSummaryButton = Object.Instantiate(exitBtn);
        GameSummaryButton.gameObject.SetActive(true);
        GameSummaryButton.sprite = TouAssets.GameSummarySprite.LoadAsset();
        GameSummaryButton.transform.position += Vector3.up * 1.65f;
        if (GameSummaryButton.transform.GetChild(1).TryGetComponent<TextTranslatorTMP>(out var tmp2))
        {
            var text = TouLocale.GetParsed("GameSummaryModeButton").Split(":");
            if (text.Length == 1 || text.Any(x => x == string.Empty))
            {
                tmp2.defaultStr = text[0];
            }
            else
            {
                tmp2.defaultStr = $"<size=70%>{text[0]}</size>\n<size=55%>{text[1]}</size>";
            }
            tmp2.TargetText = StringNames.None;
            tmp2.ResetText();
        }

        switch (LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.EndSummaryVisibility.Value)
        {
            default:
                // No summary
                roleSummary.gameObject.SetActive(false);
                roleSummary2.gameObject.SetActive(false);
                roleSummaryLeft.gameObject.SetActive(false);
                LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.Hidden;
                break;
            case EndGameSummaryVisibility.Split:
                // Split summary
                roleSummary.gameObject.SetActive(true);
                roleSummary2.gameObject.SetActive(true);
                roleSummaryLeft.gameObject.SetActive(false);
                break;
            case EndGameSummaryVisibility.LeftSide:
                // Left side summary
                roleSummary.gameObject.SetActive(false);
                roleSummary2.gameObject.SetActive(false);
                roleSummaryLeft.gameObject.SetActive(true);
                break;
        }

        var toggleAction = new Action(() =>
        {
            switch (LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.EndSummaryVisibility.Value)
            {
                case EndGameSummaryVisibility.Hidden:
                    // Split summary
                    roleSummary.gameObject.SetActive(true);
                    roleSummary2.gameObject.SetActive(true);
                    roleSummaryLeft.gameObject.SetActive(false);
                    LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.Split;
                    break;
                case EndGameSummaryVisibility.Split:
                    // Left side summary
                    roleSummary.gameObject.SetActive(false);
                    roleSummary2.gameObject.SetActive(false);
                    roleSummaryLeft.gameObject.SetActive(true);
                    LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.LeftSide;
                    break;
                case EndGameSummaryVisibility.LeftSide:
                    // No summary
                    roleSummary.gameObject.SetActive(false);
                    roleSummary2.gameObject.SetActive(false);
                    roleSummaryLeft.gameObject.SetActive(false);
                    LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.Hidden;
                    break;
            }
        });

        var passiveButton = GameSummaryButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)toggleAction);

        AfterEndGameSetup(instance);
        HandlePlayerNames();
    }

    public static void HandlePlayerNames()
    {
        PoolablePlayer[] array = Object.FindObjectsOfType<PoolablePlayer>();
        var winnerArray = EndGameResult.CachedWinners.ToArray();
        if (array.Length > 0)
        {
            foreach (var player in array)
            {
                var realPlayer = winnerArray.FirstOrDefault(x => x.PlayerName == player.cosmetics.nameText.text);
                if (realPlayer == null)
                {
                    realPlayer = winnerArray.FirstOrDefault(x => x.Outfit.HatId == player.cosmetics.hat.Hat.ProdId
                                                                 && x.Outfit.ColorId ==
                                                                 player.cosmetics
                                                                     .ColorId /*&& HatManager.Instance.GetPetById(x.Outfit.PetId) == player.cosmetics.currentPet */);
                }

                if (realPlayer == null)
                {
                    continue;
                }

                var roleType = realPlayer.RoleWhenAlive;
                var role = RoleManager.Instance.GetRole(roleType);

                if (role is JesterRole)
                {
                    player.UpdateFromPlayerOutfit(realPlayer.Outfit, PlayerMaterial.MaskType.None,
                        false, true);
                }

                var nameTxt = player.cosmetics.nameText;
                nameTxt.gameObject.SetActive(true);
                player.SetName(
                    $"\n<size=85%>{realPlayer.PlayerName}</size>\n<size=65%><color=#{role.TeamColor.ToHtmlStringRGBA()}>{role.GetRoleName()}</size>",
                    new Vector3(1.1619f, 1.1619f, 1f), Color.white, -15f);
                player.SetNamePosition(new Vector3(0f, -1.31f, -0.5f));
                nameTxt.fontSize = 1.9f;
                nameTxt.fontSizeMax = 2f;
                nameTxt.fontSizeMin = 0.5f;
                winnerArray.ToList().Remove(realPlayer);
            }
        }
        //{
        //    array[0].SetFlipX(true);

        //    array[0].gameObject.transform.position -= new Vector3(1.5f, 0f, 0f);
        //    array[0].cosmetics.skin.transform.localScale = new Vector3(-1, 1, 1);
        //    array[0].cosmetics.nameText.color = new Color(1f, 0.4f, 0.8f, 1f);
        //}
    }

    public static void AfterEndGameSetup(EndGameManager instance)
    {
        var text = Object.Instantiate(instance.WinText);
        switch (EndGameEvents.winType)
        {
            case 1:
                text.text = $"<size=4>{TouLocale.Get("CrewmatesWin")}!</size>";
                text.color = Palette.CrewmateBlue;
                instance.BackgroundBar.material.SetColor(ShaderID.Color, Palette.CrewmateBlue);
                break;
            case 2:
                text.text = $"<size=4>{TouLocale.Get("ImpostorsWin")}!</size>";
                text.color = Palette.ImpostorRed;
                instance.BackgroundBar.material.SetColor(ShaderID.Color, Palette.ImpostorRed);
                break;
            default:
                text.text = string.Empty;
                text.color = TownOfUsColors.Neutral;
                break;
        }

        var pos = instance.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);

        text.transform.position = pos;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void AmongUsClientGameEndPatch()
    {
        if (TownOfUsEventHandlers.LogBuffer.Count != 0)
        {
            foreach (var log in TownOfUsEventHandlers.LogBuffer)
            {
                var text = log.Value;
                switch (log.Key)
                {
                    case TownOfUsEventHandlers.LogLevel.Error:
                        Error(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Warning:
                        Warning(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Debug:
                        Debug(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Info:
                        Info(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Message:
                        Message(text);
                        break;
                }
            }
            TownOfUsEventHandlers.LogBuffer.Clear();
        }

        var changeRoleEvent = new ClientGameEndEvent();
        MiraEventManager.InvokeEvent(changeRoleEvent);

        BuildEndGameData();
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    [HarmonyPostfix]
    public static void EndGameManagerStart(EndGameManager __instance)
    {
        EndGameData.Clear();
    }

    public static class EndGameData
    {
        public static List<PlayerRecord> PlayerRecords { get; set; } = [];
        public static List<PlayerRecord> DisconnectedPlayerRecords { get; set; } = [];

        public static void Clear()
        {
            PlayerRecords.Clear();
        }

        public sealed class PlayerRecord
        {
            public string? ChatSummaryTitle { get; set; }
            public string? ChatSummaryRoleInfo { get; set; }
            public string? ChatSummaryStats { get; set; }
            public string? ChatSummaryCod { get; set; }
            public string? PlayerName { get; set; }
            public string? RoleString { get; set; }
            public string? RoleStringShort { get; set; }
            public bool Winner { get; set; }
            public RoleTypes LastRole { get; set; }
            public ModdedRoleTeams Team { get; set; }
            public byte PlayerId { get; set; }
        }
    }

    public static class ContainedMeetingData
    {
        public static List<PlayerMeetingRecord> PlayerMeetingRecords { get; set; } = [];

        public static void AddPlayerData(PlayerControl player)
        {
            if (PlayerMeetingRecords.Any(x => x.PlayerId == player.Data.PlayerId))
            {
                return;
            }
            Warning($"Added Meeting Record for {player.Data.PlayerName}");
            var curRound = DeathEventHandlers.CurrentRound;
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
            var taskOpt = OptionGroupSingleton<TaskTrackingOptions>.Instance;

            var causeOfDeath = $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedData").Replace("<round>", $"{curRound}")}</color>』</size>\n";
            var causeOfDeathFull = $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedDataFull").Replace("<cod>", TouLocale.Get("Alive")).Replace("<round>", $"{curRound}")}</color>』</size>\n";
            var playerName = player.Data.PlayerName ?? "Unknown";
            var playerNameColored = player.Data.PlayerName ?? "Unknown";
            var playerNameFull = player.Data.PlayerName ?? "Unknown";
            var playerNameColoredFull = player.Data.PlayerName ?? "Unknown";
            var playerColor = Color.white;
            var playerColorColored = Color.white;

            static string GetDiedR1ExtraNameTextForDisplayedIdentity(PlayerControl player)
            {
                var mod = player.GetModifiers<BaseRevealModifier>()
                    .FirstOrDefault(x => x.Visible && x is FirstRoundIndicator && x.ExtraNameText != string.Empty);
                return mod?.ExtraNameText ?? string.Empty;
            }

            var localDead = PlayerControl.LocalPlayer.HasDied();
            var localGhost = localDead && genOpt.TheDeadKnow;
            var localImp = PlayerControl.LocalPlayer.IsImpostorAligned() &&
                           genOpt is
                               { ImpsKnowRoles.Value: true, FFAImpostorMode: false };
            var localVamp = PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole;
            var useMiraApiChecks =
                !localDead && (!PlayerControl.LocalPlayer.IsImpostorAligned() || !genOpt.FFAImpostorMode);
            if (player.Data.Role != null)
            {
                var revealMods = player.GetModifiers<BaseRevealModifier>().ToList();

                if (PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() &&
                    !player.AmOwner && !genOpt.FFAImpostorMode)
                {
                    playerColorColored = Color.red;
                }

                playerColor = playerColor.UpdateTargetColor(player);
                playerName = playerName.UpdateTargetSymbols(player);
                playerName = playerName.UpdateProtectionSymbols(player);
                playerName = playerName.UpdateAllianceSymbols(player);
                playerName = playerName.UpdateStatusSymbols(player);

                playerNameFull = playerNameFull.UpdateTargetSymbols(player, DataVisibility.Show);
                playerNameFull = playerNameFull.UpdateProtectionSymbols(player, DataVisibility.Show);
                playerNameFull = playerNameFull.UpdateAllianceSymbols(player, DataVisibility.Show);
                playerNameFull = playerNameFull.UpdateStatusSymbols(player, DataVisibility.Show);

                playerColorColored = playerColorColored.UpdateTargetColor(player);
                playerNameColored = playerNameColored.UpdateTargetSymbols(player);
                playerNameColored = playerNameColored.UpdateProtectionSymbols(player);
                playerNameColored = playerNameColored.UpdateAllianceSymbols(player);
                playerNameColored = playerNameColored.UpdateStatusSymbols(player);

                playerNameColoredFull = playerNameColoredFull.UpdateTargetSymbols(player, DataVisibility.Show);
                playerNameColoredFull = playerNameColoredFull.UpdateProtectionSymbols(player, DataVisibility.Show);
                playerNameColoredFull = playerNameColoredFull.UpdateAllianceSymbols(player, DataVisibility.Show);
                playerNameColoredFull = playerNameColoredFull.UpdateStatusSymbols(player, DataVisibility.Show);

                var role = player.Data.Role;
                var customRole = player.Data.Role as ICustomRole;

                var color = role.TeamColor;

                if (HaunterRole.HaunterVisibilityFlag(player))
                {
                    playerColor = color;
                    playerColorColored = color;
                }

                var roleName = "";
                var roleNameFull = "";

                var impostorBuddy = localImp && player.IsImpostorAligned();
                var vampBuddy = localVamp && role is VampireRole;
                var revealed = revealMods.Any(x => x.Visible && x.RevealRole);
                var localFairy = FairyRole.FairySeesRoleVisibilityFlag(player);
                var localSleuth = SleuthModifier.SleuthVisibilityFlag(player);
                if (player.AmOwner || vampBuddy || impostorBuddy || revealed || localGhost || localFairy || localSleuth || useMiraApiChecks && customRole != null && customRole.CanLocalPlayerSeeRole(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color></size>";

                    var revealedRole = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
                    if (revealedRole != null)
                    {
                        color = revealedRole.ShownRole!.TeamColor;
                        roleName =
                            $"<size=80%>{color.ToTextColor()}{revealedRole.ShownRole!.GetRoleName()}</color></size>";
                    }

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole &&
                        (vampBuddy || localGhost))
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && (impostorBuddy || localGhost))
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        var cachedName = cache.CachedRoleName == "" ? cache.CachedRole.GetRoleName() : cache
                            .CachedRoleName;
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                            : $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({color.ToTextColor()}{player.Data.Role.GetRoleName()}</color>)</size>";
                    }

                    if (player.Data.IsDead && role is GuardianAngelRole gaRole)
                    {
                        roleName =
                            $"<size=80%>{gaRole.TeamColor.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    if (localSleuth || (player.Data.IsDead &&
                                        role.Role is RoleTypes.CrewmateGhost
                                            or RoleTypes.ImpostorGhost))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.GetRoleName()}</color></size>";
                        if (localDead && !player.HasModifier<VampireBittenModifier>() &&
                            roleWhenAlive is VampireRole)
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                        }

                        if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                        }
                    }

                    if (player.TryGetModifier<DeathHandlerModifier>(out var deathMod))
                    {
                        causeOfDeathFull = $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedDataFull").Replace("<cod>", deathMod.CauseOfDeath).Replace("<round>", $"{curRound}")}</color>』</size>\n";
                    }
                    roleName = $"<cod>{roleName}";
                }
                else
                {
                    causeOfDeath = $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedData").Replace("<round>", $"{curRound}")}</color>』</size>";
                    if (player.TryGetModifier<DeathHandlerModifier>(out var deathMod2))
                    {
                        causeOfDeathFull = $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedDataFull").Replace("<cod>", deathMod2.CauseOfDeath).Replace("<round>", $"{curRound}")}</color>』</size>";
                    }
                    roleName = $"<cod>";
                }

                color = role.TeamColor;
                    roleNameFull = $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color></size>";

                    var revealedRole2 = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
                    if (revealedRole2 != null)
                    {
                        color = revealedRole2.ShownRole!.TeamColor;
                        roleNameFull =
                            $"<size=80%>{color.ToTextColor()}{revealedRole2.ShownRole!.GetRoleName()}</color></size>";
                    }

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole)
                    {
                        roleNameFull += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>())
                    {
                        roleNameFull += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod2 = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod2 is ICachedRole cache2 && cache2.Visible &&
                        player.Data.Role.GetType() != cache2.CachedRole.GetType())
                    {
                        var cachedName = cache2.CachedRoleName == "" ? cache2.CachedRole.GetRoleName() : cache2
                            .CachedRoleName;
                        roleName = cache2.ShowCurrentRoleFirst
                            ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color> ({cache2.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                            : $"<size=80%>{cache2.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({color.ToTextColor()}{player.Data.Role.GetRoleName()}</color>)</size>";
                    }

                    if (player.Data.IsDead && role is GuardianAngelRole gaRole2)
                    {
                        roleNameFull =
                            $"<size=80%>{gaRole2.TeamColor.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    if (player.Data.IsDead &&
                                        role.Role is RoleTypes.CrewmateGhost
                                            or RoleTypes.ImpostorGhost)
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleNameFull = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.GetRoleName()}</color></size>";
                        if (localDead && !player.HasModifier<VampireBittenModifier>() &&
                            roleWhenAlive is VampireRole)
                        {
                            roleNameFull += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                        }

                        if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                        {
                            roleNameFull += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                        }
                    }

                    var fullCod =
                        $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedDataFull").Replace("<cod>", TouLocale.Get("Alive")).Replace("<round>", $"{curRound}")}</color>』</size>\n";
                    if (player.TryGetModifier<DeathHandlerModifier>(out var deathMod3))
                    {
                        fullCod = $"<size=60%>『{Color.yellow.ToTextColor()}{TouLocale.GetParsed("DisconnectedDataFull").Replace("<cod>", deathMod3.CauseOfDeath).Replace("<round>", $"{curRound}")}</color>』</size>\n";
                    }
                    roleNameFull = $"{fullCod}{roleNameFull}";

                var revealedColorMod = revealMods.FirstOrDefault(x => x.Visible && x.NameColor != null);
                if (revealedColorMod != null)
                {
                    playerColor = (Color)revealedColorMod.NameColor!;
                    playerName = $"{playerColor.ToTextColor()}{playerName}</color>";
                    playerNameFull = $"{playerColor.ToTextColor()}{playerNameFull}</color>";
                    playerColorColored = (Color)revealedColorMod.NameColor!;
                    playerNameColored = $"{playerColorColored.ToTextColor()}{playerNameColored}</color>";
                    playerNameColoredFull = $"{playerColorColored.ToTextColor()}{playerNameColoredFull}</color>";
                }

                var addedRoleNameText = revealMods.FirstOrDefault(x => x.Visible && x.ExtraRoleText != string.Empty);
                if (addedRoleNameText != null)
                {
                    roleName += $"<size=80%>{addedRoleNameText.ExtraRoleText}</size>";
                    roleNameFull += $"<size=80%>{addedRoleNameText.ExtraRoleText}</size>";
                }

                if (((taskOpt.ShowTaskInMeetings && player.AmOwner) ||
                     (localDead && taskOpt.ShowTaskDead)) &&
                    (player.IsCrewmate() || player.Data.Role is SpectreRole))
                {
                    if (roleName != string.Empty)
                    {
                        roleName += " ";
                    }

                    roleName += $"<size=80%>{player.TaskInfo()}</size>";
                }

                if (taskOpt.ShowTaskDead && (player.IsCrewmate() || player.Data.Role is SpectreRole))
                {
                    if (roleNameFull != string.Empty)
                    {
                        roleNameFull += " ";
                    }

                    roleNameFull += $"<size=80%>{player.TaskInfo()}</size>";
                }

                if (player.TryGetModifier<OracleConfessModifier>(out var confess, x => x.ConfessToAll))
                {
                    var accuracy = OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage;
                    var revealText = confess.RevealedFaction switch
                    {
                        ModdedRoleTeams.Crewmate =>
                            $"\n<size=75%>{Palette.CrewmateBlue.ToTextColor()}({accuracy}% Crew) </color></size>",
                        ModdedRoleTeams.Custom =>
                            $"\n<size=75%>{TownOfUsColors.Neutral.ToTextColor()}({accuracy}% Neut) </color></size>",
                        ModdedRoleTeams.Impostor =>
                            $"\n<size=75%>{TownOfUsColors.ImpSoft.ToTextColor()}({accuracy}% Imp) </color></size>",
                        _ => string.Empty
                    };

                    playerName += revealText;
                    playerNameColored += revealText;
                    playerNameFull += revealText;
                    playerNameColoredFull += revealText;
                }

                var addedPlayerNameText = revealMods.FirstOrDefault(x =>
                    x.Visible && x.ExtraNameText != string.Empty && x is not FirstRoundIndicator);
                if (addedPlayerNameText != null)
                {
                    playerName += addedPlayerNameText.ExtraNameText;
                    playerNameColored += addedPlayerNameText.ExtraNameText;
                    playerNameFull += addedPlayerNameText.ExtraNameText;
                    playerNameColoredFull += addedPlayerNameText.ExtraNameText;
                }

                var diedR1Text = GetDiedR1ExtraNameTextForDisplayedIdentity(player);
                if (!string.IsNullOrEmpty(diedR1Text))
                {
                    playerName += diedR1Text;
                    playerNameColored += diedR1Text;
                    playerNameFull += diedR1Text;
                    playerNameColoredFull += diedR1Text;
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    playerNameColored = $"{roleName}\n{playerColor.ToTextColor()}<size=92%>{playerNameColored}</size></color>";
                    playerName = $"{roleName}\n<size=92%>{playerName}</size>";
                }

                if (!string.IsNullOrEmpty(roleNameFull))
                {
                    playerNameColoredFull = $"{roleNameFull}\n{color.ToTextColor()}<size=92%>{playerNameColoredFull}</size></color>";
                    playerNameFull = $"{roleNameFull}\n<size=92%>{playerNameFull}</size>";
                }

                playerNameColoredFull = playerNameColoredFull.Replace("<cod>", causeOfDeathFull);
                playerNameFull = playerNameFull.Replace("<cod>", causeOfDeathFull);
                playerNameColored = playerNameColored.Replace("<cod>", causeOfDeath);
                playerName = playerName.Replace("<cod>", causeOfDeath);
            }

            PlayerMeetingRecords.Add(new PlayerMeetingRecord
            {
                PlayerNameUncolored = playerName,
                PlayerNameColored = playerNameColored,
                PlayerNameUncoloredFull = playerNameFull,
                PlayerNameColoredFull = playerNameColoredFull,
                PlayerColorColored = playerColorColored,
                PlayerColorUncolored = playerColor,
                PlayerId = player.Data.PlayerId
            });
        }

        public static void DisplayRecordData(TextMeshPro tmp, PlayerMeetingRecord record, bool color, bool isLocalDead)
        {
            if (color)
            {
                tmp.text = isLocalDead ? record.PlayerNameColoredFull : record.PlayerNameColored;
                tmp.color = record.PlayerColorColored;
            }
            else
            {
                tmp.text = isLocalDead ? record.PlayerNameUncoloredFull : record.PlayerNameUncolored;
                tmp.color = record.PlayerColorUncolored;
            }
        }

        public static void Clear()
        {
            PlayerMeetingRecords.Clear();
        }

        public sealed class PlayerMeetingRecord
        {
            public string PlayerNameUncolored { get; set; }
            public string PlayerNameColored { get; set; }
            public string PlayerNameUncoloredFull { get; set; }
            public string PlayerNameColoredFull { get; set; }
            public Color PlayerColorUncolored { get; set; }
            public Color PlayerColorColored { get; set; }
            public byte PlayerId { get; set; }
        }
    }
}