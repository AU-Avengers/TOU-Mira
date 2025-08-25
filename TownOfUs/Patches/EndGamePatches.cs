using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
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

        var playerRoleString = new StringBuilder();

        // Theres a better way of doing this e.g. switch statement or dictionary. But this works for now.
        foreach (var playerControl in PlayerControl.AllPlayerControls)
        {
            playerRoleString.Clear();

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

                if (!string.IsNullOrEmpty(role.NiceName.Trim()))
                {
                    roleName = role.NiceName;
                }
                else
                {
                    roleName = role.Player.IsImpostor() ? "Impostor" : "Crewmate";
                }

                playerRoleString.Append(TownOfUsPlugin.Culture, $"{color.ToTextColor()}{roleName}</color> > ");
            }

            if (playerRoleString.Length > 3)
            {
                playerRoleString = playerRoleString.Remove(playerRoleString.Length - 3, 3);
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
                if (modifiers.FirstOrDefault(x => x.ModifierName == modifierName) is IColoredModifier colorMod)
                {
                    modColor = colorMod.ModifierColor;
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

            if (playerControl.IsRole<PhantomTouRole>() || playerTeam == ModdedRoleTeams.Crewmate)
            {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" {playerControl.TaskInfo()}");
            }

            var killedPlayers = GameHistory.KilledPlayers.Count(x =>
                x.KillerId == playerControl.PlayerId && x.VictimId != playerControl.PlayerId);

            if (killedPlayers > 0 && !playerControl.IsCrewmate() && !playerControl.Is(RoleAlignment.NeutralEvil))
            {
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" |{TownOfUsColors.Impostor.ToTextColor()} Kills: {killedPlayers}</color>");
            }

            if (GameHistory.PlayerStats.TryGetValue(playerControl.PlayerId, out var stats))
            {
                if (killedPlayers > 0 && playerControl.IsCrewmate() && stats.CorrectKills <= 0 &&
                    stats.IncorrectKills <= 0 && !playerControl.Is(RoleAlignment.NeutralEvil))
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" |{TownOfUsColors.Impostor.ToTextColor()} Kills: {killedPlayers}</color>");
                }

                if (stats.CorrectKills > 0)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}Kills: {stats.CorrectKills}</color>");
                }

                if (stats.IncorrectKills > 0)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}Mis-kills: {stats.IncorrectKills}</color>");
                }

                if (stats.CorrectAssassinKills > 0)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {Color.green.ToTextColor()}Guesses: {stats.CorrectAssassinKills}</color>");
                }

                if (stats.IncorrectAssassinKills > 0)
                {
                    playerRoleString.Append(TownOfUsPlugin.Culture,
                        $" | {TownOfUsColors.Impostor.ToTextColor()}Misguesses: {stats.IncorrectAssassinKills}</color>");
                }
            }
            if (playerControl.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
            {
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}{deathHandler.CauseOfDeath}</color>");
                if (deathHandler.KilledBy != string.Empty) playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" {deathHandler.KilledBy}");
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" (R{deathHandler.RoundOfDeath})");
            }
            else
            {
                playerRoleString.Append(TownOfUsPlugin.Culture,
                    $" | {Color.yellow.ToTextColor()}Alive</color>");
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

            var alliance = playerControl.GetModifiers<AllianceGameModifier>().FirstOrDefault();
            if (alliance != null)
            {
                var modColor = MiscUtils.GetRoleColour(alliance.ModifierName.Replace(" ", string.Empty));
                if (alliance is IColoredModifier colorMod)
                {
                    modColor = colorMod.ModifierColor;
                }

                playerName.Append(TownOfUsPlugin.Culture,
                    $" <b>{modColor.ToTextColor()}<size=60%>{alliance.Symbol}</size></color></b>");
            }

            EndGameData.PlayerRecords.Add(new EndGameData.PlayerRecord
            {
                PlayerName = playerName.ToString(),
                RoleString = playerRoleString.ToString(),
                Winner = playerWinner,
                LastRole = playerRoleType,
                Team = playerTeam
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
        var roleSummaryBackup = new StringBuilder();
        roleSummaryText1.AppendLine("End game summary:");
        roleSummaryTextFull.AppendLine("End game summary:");
        var count = 0;
        foreach (var data in EndGameData.PlayerRecords)
        {
            var role = string.Join(" ", data.RoleString);
            if (count % 2 == 0)
            {
                roleSummaryText2.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role}");
            }
            else
            {
                roleSummaryText1.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role}");
            }

            count++;
            roleSummaryBackup.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role}");
            roleSummaryTextFull.AppendLine(TownOfUsPlugin.Culture, $"{data.PlayerName} - {role}");
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

        GameHistory.EndGameSummary = roleSummaryBackup.ToString();

        var GameSummaryButton = Object.Instantiate(exitBtn);
        GameSummaryButton.gameObject.SetActive(true);
        GameSummaryButton.sprite = TouAssets.GameSummarySprite.LoadAsset();
        GameSummaryButton.transform.position += Vector3.up * 1.65f;
        if (GameSummaryButton.transform.GetChild(1).TryGetComponent<TextTranslatorTMP>(out var tmp2))
        {
            tmp2.defaultStr = "<size=70%>Game</size>\n<size=55%>Summary</size>";
            tmp2.TargetText = StringNames.None;
            tmp2.ResetText();
        }

        switch (TownOfUsPlugin.GameSummaryMode.Value)
        {
            default:
                // No summary
                roleSummary.gameObject.SetActive(false);
                roleSummary2.gameObject.SetActive(false);
                roleSummaryLeft.gameObject.SetActive(false);
                TownOfUsPlugin.GameSummaryMode.Value = 0;
                break;
            case 1:
                // Split summary
                roleSummary.gameObject.SetActive(true);
                roleSummary2.gameObject.SetActive(true);
                roleSummaryLeft.gameObject.SetActive(false);
                break;
            case 2:
                // Left side summary
                roleSummary.gameObject.SetActive(false);
                roleSummary2.gameObject.SetActive(false);
                roleSummaryLeft.gameObject.SetActive(true);
                break;
        }

        var toggleAction = new Action(() =>
        {
            switch (TownOfUsPlugin.GameSummaryMode.Value)
            {
                case 0:
                    // Split summary
                    roleSummary.gameObject.SetActive(true);
                    roleSummary2.gameObject.SetActive(true);
                    roleSummaryLeft.gameObject.SetActive(false);
                    TownOfUsPlugin.GameSummaryMode.Value = 1;
                    break;
                case 1:
                    // Left side summary
                    roleSummary.gameObject.SetActive(false);
                    roleSummary2.gameObject.SetActive(false);
                    roleSummaryLeft.gameObject.SetActive(true);
                    TownOfUsPlugin.GameSummaryMode.Value = 2;
                    break;
                case 2:
                    // No summary
                    roleSummary.gameObject.SetActive(false);
                    roleSummary2.gameObject.SetActive(false);
                    roleSummaryLeft.gameObject.SetActive(false);
                    TownOfUsPlugin.GameSummaryMode.Value = 0;
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
                    $"\n<size=85%>{realPlayer.PlayerName}</size>\n<size=65%><color=#{role.TeamColor.ToHtmlStringRGBA()}>{role.NiceName}</size>",
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
                text.text = "<size=4>Crewmates Win!</size>";
                text.color = Palette.CrewmateBlue;
                instance.BackgroundBar.material.SetColor(ShaderID.Color, Palette.CrewmateBlue);
                break;
            case 2:
                text.text = "<size=4>Impostors Win!</size>";
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

        public sealed class PlayerRecord
        {
            public string? PlayerName { get; set; }
            public string? RoleString { get; set; }
            public bool Winner { get; set; }
            public RoleTypes LastRole { get; set; }
            public ModdedRoleTeams Team { get; set; }
        }
    }
}