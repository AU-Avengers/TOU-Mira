using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System.Text;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using TMPro;
using TownOfUs.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static TownOfUs.Modules.Components.HudManagerHelper;

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
        foreach (var playerStats in GameHistory.PlayerStats.Values)
        {
            playerRoleString.Clear();
            playerRoleStringShort.Clear();
            summaryTitle.Clear();
            summaryRoleInfo.Clear();
            summaryStats.Clear();
            summaryCod.Clear();
            if (playerStats.IsSpectator)
            {
                EndGameData.PlayerRecords.Add(new EndGameData.PlayerRecord
                {
                    ChatSummaryTitle = $"{playerStats.PlayerName} - {MiscUtils.GetRoleTmpIcon((RoleTypes)RoleId.Get<SpectatorRole>())}{MiraLocaleManager.Get("TouRoleSpectator")}",
                    ChatSummaryRoleInfo = string.Empty,
                    ChatSummaryStats = string.Empty,
                    ChatSummaryCod = string.Empty,
                    PlayerName = playerStats.PlayerName,
                    RoleString = MiscUtils.GetRoleTmpIcon((RoleTypes)RoleId.Get<SpectatorRole>()) + MiraLocaleManager.Get("TouRoleSpectator"),
                    RoleStringShort = MiscUtils.GetRoleTmpIcon((RoleTypes)RoleId.Get<SpectatorRole>()) + MiraLocaleManager.Get("TouRoleSpectator"),
                    Winner = false,
                    LastRole = (RoleTypes)RoleId.Get<SpectatorRole>(),
                    Team = ModdedRoleTeams.Custom,
                    PlayerId = playerStats.PlayerId
                });
                continue;
            }

            var latestRole = string.Empty;
            var changedAgain = false;

            var lastRole = playerStats.DisplayedRole;
            foreach (var role in playerStats.TrackedRoles)
            {
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

                roleName = $"{MiscUtils.GetRoleTmpIcon(role)}{roleName}";

                if (latestRole != string.Empty)
                {
                    changedAgain = true;
                }
                latestRole = $"{color.ToTextColor()}{roleName}</color>";
                // lastRole = role;

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
            var playerTeam = ModdedRoleTeams.Crewmate;

            if (lastRole is ITownOfUsRole touRole)
            {
                playerTeam = touRole.Team;
            }
            else if (lastRole.IsImpostor)
            {
                playerTeam = ModdedRoleTeams.Impostor;
            }

            var modifiers = playerStats.LastKnownModifiers
                .Where(x => x is TouGameModifier touMod && touMod.AppearsInSummary || x is UniversalGameModifier);
            var modifierCount = modifiers.Count();
            if (modifierCount != 0)
            {
                playerRoleString.Append(TownOfUsPlugin.Culture, $" (");
            }

            foreach (var modifier in modifiers)
            {
                var modColor = MiscUtils.GetModifierColour(modifier);

                modifierCount--;
                if (modifierCount == 0)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture, $"{modColor.ToTextColor()}{modifier.ModifierName}</color>)");
                }
                else
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $"{modColor.ToTextColor()}{modifier.ModifierName}</color>, ");
                }
            }
            var modifierHolder = new StringBuilder();
            var modifiersAlt = playerStats.LastKnownModifiers
                .Where(x => x is TouGameModifier touMod && touMod.AppearsInSummary || x is UniversalGameModifier || x is AllianceGameModifier);
            var modifierCountAlt = modifiersAlt.Count();
            if (modifierCountAlt != 0)
            {
                modifierHolder.Append(TownOfUsPlugin.Culture, $" (");
            }

            foreach (var modifier in modifiersAlt)
            {
                var modColor = MiscUtils.GetModifierColour(modifier);

                modifierCountAlt--;
                if (modifierCountAlt == 0)
                {
                    modifierHolder.Append(TownOfUsPlugin.Culture, $"{modColor.ToTextColor()}{modifier.ModifierName}</color>)");
                }
                else
                {
                    modifierHolder.Append(TownOfUsPlugin.Culture,
                        $"{modColor.ToTextColor()}{modifier.ModifierName}</color>, ");
                }
            }

            if (playerStats.DisplayedRole is IProgressTally tally)
            {
                if (tally.ProgressOnSummaryNormal != string.Empty)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" {tally.ProgressOnSummaryNormal}");
                }

                if (tally.ProgressOnSummaryDetailed != string.Empty)
                {
                    summaryStats.Append(TownOfUsPlugin.Culture, $" | {tally.ProgressOnSummaryDetailed}");
                }
            }
            else if (playerTeam == ModdedRoleTeams.Crewmate && playerStats.PlayerState != StoredPlayerState.Disconnected)
            {
                var playerControl = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == playerStats.PlayerId);
                if (playerControl != null)
                {
                    var taskInfo = playerControl.TaskInfo();
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" {taskInfo}");
                    summaryStats.Append(TownOfUsPlugin.Culture, $" | {MiraLocaleManager.Get("StatsTaskCount").Replace("<count>", taskInfo.Replace("(", "").Replace(")", ""))}");
                }
            }

            var killedPlayers = GameHistory.KilledPlayers.Count(x =>
                x.KillerId == playerStats.PlayerId && x.VictimId != playerStats.PlayerId);

            if (GameHistory.PlayerStats.TryGetValue(playerStats.PlayerId, out var stats))
            {
                var basicKillCount = killedPlayers - stats.CorrectAssassinKills - stats.IncorrectKills - stats.IncorrectAssassinKills - stats.CorrectKills;
                if (stats.CorrectKills > 0)
                {
                    summaryStats.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}{MiraLocaleManager.Get("StatsKillCount").Replace("<count>", $"{stats.CorrectKills}")}</color>");
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}{MiraLocaleManager.Get("StatsKillCount").Replace("<count>", $"{stats.CorrectKills}")}</color>");
                }
                else if (basicKillCount > 0 && !playerStats.DisplayedRole.IsCrewmate())
                {
                    summaryStats.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsKillCount").Replace("<count>", $"{basicKillCount}")}</color>");
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsKillCount").Replace("<count>", $"{basicKillCount}")}</color>");
                }

                if (stats.IncorrectKills > 0)
                {
                    summaryStats.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsBadKillCount").Replace("<count>", $"{stats.IncorrectKills}")}</color>");
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsBadKillCount").Replace("<count>", $"{stats.IncorrectKills}")}</color>");
                }

                if (stats.CorrectAssassinKills > 0)
                {
                    summaryStats.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}{MiraLocaleManager.Get("StatsGoodGuessCount").Replace("<count>", $"{stats.CorrectAssassinKills}")}</color>");
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}{MiraLocaleManager.Get("StatsGoodGuessCount").Replace("<count>", $"{stats.CorrectAssassinKills}")}</color>");
                }

                /*if (stats.IncorrectAssassinKills > 0)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsBadGuessCount").Replace("<count>", $"{stats.IncorrectAssassinKills}")}</color>");
                }*/
            }
            else if (killedPlayers > 0 && !playerStats.DisplayedRole.IsCrewmate() && playerStats.DisplayedRole.GetRoleAlignment() != RoleAlignment.NeutralEvil)
            {
                summaryStats.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsKillCount").Replace("<count>", $"{killedPlayers}")}</color>");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("StatsKillCount").Replace("<count>", $"{killedPlayers}")}</color>");
            }

            playerRoleStringShort.Append(playerRoleString);

                var hasExtendedCauseOfDeath = !string.IsNullOrEmpty(playerStats.ExtendedCauseOfDeath);
                var causeOfDeath = hasExtendedCauseOfDeath
                    ? playerStats.ExtendedCauseOfDeath
                    : playerStats.DeathString;

                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}{causeOfDeath}</color>");
                playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}{playerStats.DeathString}</color>");
                summaryCod.Append(TownOfUsPlugin.Culture,
                    $"{Color.yellow.ToTextColor()}{causeOfDeath}</color>");
                if (!hasExtendedCauseOfDeath && playerStats.KilledBy != string.Empty)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" {playerStats.KilledBy}");
                    summaryCod.Append(TownOfUsPlugin.Culture,
                        $" {playerStats.KilledBy}");
                }

                if (playerStats.RoundOfDeath != -1)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" ({MiraLocaleManager.Get("RoundOfDeath").Replace("<count>", $"{playerStats.RoundOfDeath}")})");

                    playerRoleStringShort.Append(TownOfUsPlugin.Culture,
                        $" ({MiraLocaleManager.Get("RoundOfDeath").Replace("<count>", $"{playerStats.RoundOfDeath}")})");

                    summaryCod.Append(TownOfUsPlugin.Culture,
                        $" ({MiraLocaleManager.Get("RoundOfDeathLong").Replace("<count>", $"{playerStats.RoundOfDeath}")})");
                }

            var playerName = new StringBuilder();
            var playerWinner = false;

            if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == playerStats.PlayerName))
            {
                playerName.Append(TownOfUsPlugin.Culture, $"<color=#EFBF04>{playerStats.PlayerName}</color>");
                playerWinner = true;
            }
            else
            {
                playerName.Append(playerStats.PlayerName);
            }
            summaryTitle.Append(TownOfUsPlugin.Culture, $"{playerName.ToString()} - {latestRole}{modifierHolder.ToString()}");

            var alliance = playerStats.LastKnownModifiers.OfType<AllianceGameModifier>().FirstOrDefault();
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
                LastRole = lastRole.Role,
                Team = playerTeam,
                PlayerId = playerStats.PlayerId
            });
        }
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
        var summaryTxt = MiraLocaleManager.Get("EndGameSummary") + ":";
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
            var text = MiraLocaleManager.Get("GameSummaryModeButton").Split(":");
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

        switch (LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility.Value)
        {
            default:
                // No summary
                roleSummary.gameObject.SetActive(false);
                roleSummary2.gameObject.SetActive(false);
                roleSummaryLeft.gameObject.SetActive(false);
                LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.Hidden;
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
            switch (LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility.Value)
            {
                case EndGameSummaryVisibility.Hidden:
                    // Split summary
                    roleSummary.gameObject.SetActive(true);
                    roleSummary2.gameObject.SetActive(true);
                    roleSummaryLeft.gameObject.SetActive(false);
                    LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.Split;
                    break;
                case EndGameSummaryVisibility.Split:
                    // Left side summary
                    roleSummary.gameObject.SetActive(false);
                    roleSummary2.gameObject.SetActive(false);
                    roleSummaryLeft.gameObject.SetActive(true);
                    LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.LeftSide;
                    break;
                case EndGameSummaryVisibility.LeftSide:
                    // No summary
                    roleSummary.gameObject.SetActive(false);
                    roleSummary2.gameObject.SetActive(false);
                    roleSummaryLeft.gameObject.SetActive(false);
                    LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility.Value = EndGameSummaryVisibility.Hidden;
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
                realPlayer ??= winnerArray.FirstOrDefault(x => x.Outfit.HatId == player.cosmetics.hat.Hat.ProdId
                                                                 && x.Outfit.ColorId ==
                                                                 player.cosmetics
                                                                     .ColorId);

                if (realPlayer == null)
                {
                    continue;
                }
                var actualRole = RoleManager.Instance.GetRole(realPlayer.RoleWhenAlive);
                var realDealPlayer = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.CurrentOutfit == realPlayer.Outfit);
                if (realDealPlayer != null)
                {
                    foreach (var role in GameHistory.RoleHistory.Where(x => x.Key == realDealPlayer.PlayerId)
                                 .Select(x => x.Value))
                    {
                        if (role.Role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost ||
                            role.Role == (RoleTypes)RoleId.Get<NeutralGhostRole>())
                        {
                            continue;
                        }
                        actualRole = role;
                    }
                }

                if (actualRole is JesterRole)
                {
                    player.UpdateFromPlayerOutfit(realPlayer.Outfit, PlayerMaterial.MaskType.None,
                        false, true);
                }
                else if (actualRole is IGhostRole)
                {
                    player.UpdateFromPlayerOutfit(realPlayer.Outfit, PlayerMaterial.MaskType.None,
                        false, true);
                    foreach (var renderer in player.Cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
                    {
                        var col = renderer.color;
                        col.a = 0.5f;
                        renderer.color = col;
                    }
                    var col2 = player.Cosmetics.currentBodySprite.BodySprite.color;
                    col2.a = 0.5f;
                    player.Cosmetics.currentBodySprite.BodySprite.color = col2;
                    if (player.Cosmetics.bodySprites.Count > 0)
                    {
                        foreach (var body in player.Cosmetics.bodySprites)
                        {
                            var renderer = body.BodySprite;
                            var col = renderer.color;
                            col.a = 0.5f;
                            renderer.color = col;
                        }
                    }
                }

                var nameTxt = player.cosmetics.nameText;
                nameTxt.gameObject.SetActive(true);
                player.SetName(
                    $"\n<size=85%>{realPlayer.PlayerName}</size>\n<size=65%><color=#{actualRole.TeamColor.ToHtmlStringRGBA()}>{MiscUtils.GetRoleTmpIcon(actualRole)}{actualRole.GetRoleName()}</size>",
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
                text.text = $"<size=4>{MiraLocaleManager.Get("CrewmatesWin")}!</size>";
                text.color = Palette.CrewmateBlue;
                instance.BackgroundBar.material.SetColor(ShaderID.Color, Palette.CrewmateBlue);
                break;
            case 2:
                text.text = $"<size=4>{MiraLocaleManager.Get("ImpostorsWin")}!</size>";
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

        public static void Clear()
        {
            PlayerRecords.Clear();
        }

        public sealed record PlayerRecord
        {
            public string? ChatSummaryTitle { get; init; }
            public string? ChatSummaryRoleInfo { get; init; }
            public string? ChatSummaryStats { get; init; }
            public string? ChatSummaryCod { get; init; }
            public string? PlayerName { get; init; }
            public string? RoleString { get; init; }
            public string? RoleStringShort { get; init; }
            public bool Winner { get; init; }
            public RoleTypes LastRole { get; init; }
            public ModdedRoleTeams Team { get; init; }
            public byte PlayerId { get; init; }
        }
    }

    public static class ContainedMeetingData
    {
        public static List<PlayerMeetingRecord> PlayerMeetingRecords { get; set; } = [];

        private static string GetCauseOfDeathString(string parsedData)
        {
            var curRound = HudManagerHelper.Instance.CurrentRound;
            return $"<size=60%>『{Color.yellow.ToTextColor()}{parsedData.Replace("<round>", $"{curRound}")}</color>』</size>";
        }

        public static void AddPlayerData(PlayerControl player)
        {
            if (PlayerMeetingRecords.Any(x => x.PlayerId == player.Data.PlayerId))
            {
                return;
            }
            var state = GameHistory.PlayerStats[player.PlayerId];

            Warning($"Added Meeting Record for {player.Data.PlayerName}");

            var causeOfDeath = GetCauseOfDeathString(MiraLocaleManager.Get("DisconnectedData"));
            var causeOfDeathFull =
                GetCauseOfDeathString(MiraLocaleManager.Get("DisconnectedDataFull")
                    .Replace("<cod>", state.DeathString));
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
            var taskOpt = OptionGroupSingleton<PostmortemOptions>.Instance;

            var roleNameSize = HudManagerPatches.RoleIsSmall ? "80%" : "100%";
            var roleOnTop = HudManagerPatches.RoleOnTop;

            var localDead = PlayerControl.LocalPlayer.HasDied();
            var localGhost = localDead && genOpt.TheDeadKnow;
            var localImp = PlayerControl.LocalPlayer.IsImpostorAligned() &&
                           genOpt is
                               { ImpsKnowRoles.Value: true, FFAImpostorMode: false };
            var localVamp = PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole;
            var useMiraApiChecks =
                !localDead && (!PlayerControl.LocalPlayer.IsImpostorAligned() || !genOpt.FFAImpostorMode);
            var (playerColor, playerName) = GetRoleNameText(player, genOpt.FFAImpostorMode, taskOpt, roleNameSize,
                roleOnTop, false, localDead, localGhost, localImp, localVamp, useMiraApiChecks, true, true, true);
            playerName = playerName.Replace("<cod>", causeOfDeath);
            var (playerColorColored, playerNameColored) = GetRoleNameText(player, genOpt.FFAImpostorMode, taskOpt,
                roleNameSize, roleOnTop, true, localDead, localGhost, localImp, localVamp, useMiraApiChecks, true, true,
                true);
            playerNameColored = playerNameColored.Replace("<cod>", causeOfDeath);
            var (playerColorFull, playerNameFull) = GetRoleNameText(player, genOpt.FFAImpostorMode, taskOpt,
                roleNameSize, roleOnTop, false, true, true, localImp, localVamp, useMiraApiChecks, true, true, true);
            playerNameFull = playerNameFull.Replace("<cod>", causeOfDeathFull);
            var (playerColorColoredFull, playerNameColoredFull) = GetRoleNameText(player, genOpt.FFAImpostorMode,
                taskOpt, roleNameSize, roleOnTop, true, true, true, localImp, localVamp, useMiraApiChecks, true, true,
                true);
            playerNameColoredFull = playerNameColoredFull.Replace("<cod>", causeOfDeathFull);
            PlayerMeetingRecords.Add(new PlayerMeetingRecord
            {
                PlayerNameUncolored = playerName,
                PlayerColorUncolored = playerColor,
                PlayerNameColored = playerNameColored,
                PlayerColorColored = playerColorColored,
                PlayerNameUncoloredFull = playerNameFull,
                PlayerColorUncoloredFull = playerColorFull,
                PlayerNameColoredFull = playerNameColoredFull,
                PlayerColorColoredFull = playerColorColoredFull,
                PlayerId = player.Data.PlayerId
            });
        }

        public static void DisplayRecordData(TextMeshPro tmp, PlayerMeetingRecord record, bool color, bool isLocalDead)
        {
            if (color)
            {
                tmp.text = isLocalDead ? record.PlayerNameColoredFull : record.PlayerNameColored;
                tmp.color = isLocalDead ? record.PlayerColorColoredFull : record.PlayerColorColored;
            }
            else
            {
                tmp.text = isLocalDead ? record.PlayerNameUncoloredFull : record.PlayerNameUncolored;
                tmp.color = isLocalDead ? record.PlayerColorUncoloredFull : record.PlayerColorUncolored;
            }
            if (tmp.m_lineNumber > 1)
            {
                tmp.fontSize = 2f - tmp.m_lineNumber * 0.075f;
            }
            else
            {
                tmp.fontSize = 2f;
            }
        }

        public static void Clear()
        {
            PlayerMeetingRecords.Clear();
        }

        public sealed record PlayerMeetingRecord
        {
            public string PlayerNameUncolored { get; init; }
            public Color PlayerColorUncolored { get; init; }
            public string PlayerNameColored { get; init; }
            public Color PlayerColorColored { get; init; }
            public string PlayerNameUncoloredFull { get; init; }
            public Color PlayerColorUncoloredFull { get; init; }
            public string PlayerNameColoredFull { get; init; }
            public Color PlayerColorColoredFull { get; init; }
            public byte PlayerId { get; init; }
        }
    }
}