using System.Reflection;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Patches.Options;
using TownOfUs.Patches.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]
public static class ChatPatches
{
    private static readonly char[] separator = [' '];
    public static string GetLobbyRulesText()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(dir)) return string.Empty;
        var path = Path.Combine(dir, "LobbyRules.txt");
        if (!File.Exists(path)) return string.Empty;
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return string.Empty;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ForcePlayerRole)]
    public static void RpcForcePlayerRole(PlayerControl host, PlayerControl player)
    {
        if (host.AmOwner)
        {
            return;
        }

        var systemName = $"<color=#8BFDFD>{TouLocale.GetParsed("SystemChatTitle")}</color>";
        MiscUtils.AddSystemChat(host.Data, systemName,
            TouLocale.GetParsed("UpCommandSuccessGlobal").Replace("<player>", player.Data.PlayerName));
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(FriendsListManager), nameof(FriendsListManager.SetFriendButtonColor))]
    public static bool SetFriendButtonColor(FriendsListManager __instance, bool isGrayedOut)
    {
        __instance.FriendsListButton?.SetGlyphColor(isGrayedOut);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Close))]
    public static void TogglePrefix(ChatController __instance)
	{
        __instance.chatButton.transform.localPosition = HudManagerPatches.ClonedChatButton.transform.localPosition + new Vector3(-0.3f, 0);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatWarning))]
    public static void ChatBubbleUpdatePrefix(ChatController __instance)
    {
        __instance.chatNotifyDot.transform.localPosition = new Vector3(-0.34f, 0.373f, -1f);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static bool FirstPrefix(ChatController __instance)
    {
        var text = __instance.freeChatField.Text.ToLower(TownOfUsPlugin.Culture);
        var textRegular = __instance.freeChatField.Text.WithoutRichText();

        // Remove chat limit
        if (textRegular.Length < 1)
        {
            return true;
        }

        var systemName = $"<color=#8BFDFD>{TouLocale.GetParsed("SystemChatTitle")}</color>";
        var specCommandList = TouLocale.GetParsed("SpectatorCommandList").Split(":");
        var summaryCommandList = TouLocale.GetParsed("SummaryCommandList").Split(":");
        var rolesCommandList = TouLocale.GetParsed("RolesCommandList").Split(":");
        var nerfCommandList = TouLocale.GetParsed("NerfMeCommandList").Split(":");
        var nameCommandList = TouLocale.GetParsed("SetNameCommandList").Split(":");
        var helpCommandList = TouLocale.GetParsed("HelpCommandList").Split(":");
        var upCommandList = TouLocale.GetParsed("UpCommandList").Split(":");
        var rulesCommandList = TouLocale.GetParsed("RulesCommandList").Split(":");
        var infoCommandList = TouLocale.GetParsed("InfoCommandList").Split(":");

        if (TranslationController.InstanceExists &&
            TranslationController.Instance.currentLanguage.languageID is not SupportedLangs.English)
        {
            specCommandList = specCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "SpectatorCommandList").Split(":"));
            summaryCommandList = summaryCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "SummaryCommandList").Split(":"));
            rolesCommandList = rolesCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "RolesCommandList").Split(":"));
            nerfCommandList = nerfCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "NerfMeCommandList").Split(":"));
            nameCommandList = nameCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "SetNameCommandList").Split(":"));
            helpCommandList = helpCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "HelpCommandList").Split(":"));
            upCommandList = upCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "UpCommandList").Split(":"));
            rulesCommandList = rulesCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "RulesCommandList").Split(":"));
            infoCommandList = infoCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "InfoCommandList").Split(":"));
        }

        var spaceLess = text.Replace(" ", string.Empty);
        if (specCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            if (!LobbyBehaviour.Instance)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    TouLocale.GetParsed("SpectatorLobbyError"));
            }
            else
            {
                if (GameStartManager.InstanceExists &&
                    GameStartManager.Instance.startState is GameStartManager.StartingStates.Countdown)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorStartError"));
                }
                else if (SpectatorRole.TrackedSpectators.Contains(PlayerControl.LocalPlayer.Data.PlayerName))
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorToggleOff"));
                    RpcRemoveSpectator(PlayerControl.LocalPlayer);
                }
                else if (!OptionGroupSingleton<HostSpecificOptions>.Instance.EnableSpectators)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorHostError"));
                }
                else
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorToggleOn"));
                    RpcSelectSpectator(PlayerControl.LocalPlayer);
                }
            }

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (spaceLess.StartsWith("/", StringComparison.OrdinalIgnoreCase)
            && summaryCommandList.Any(x => spaceLess.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            systemName = $"<color=#8BFDFD>{TouLocale.Get("EndGameSummary")}</color>";
            var title = systemName;
            var msg = TouLocale.GetParsed("SummaryMissingError");
            var summary = GameHistory.EndGameSummary;
            switch (LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.SummaryMessageAppearance.Value)
            {
                case GameSummaryAppearance.Advanced:
                    summary = GameHistory.EndGameSummaryAdvanced;
                    break;
                case GameSummaryAppearance.Simplified:
                    summary = GameHistory.EndGameSummarySimple;
                    break;
            }
            if (summary != string.Empty)
            {
                var factionText = string.Empty;
                if (GameHistory.WinningFaction != string.Empty)
                {
                    factionText =
                        $"<size=80%>{TouLocale.GetParsed("EndResult").Replace("<victoryType>", GameHistory.WinningFaction)}</size>\n";
                }

                title = $"{systemName}\n<size=62%>{factionText}{summary}</size>";
                msg = string.Empty;
            }

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        // Adds /kick
        if (textRegular.StartsWith("/kick ", StringComparison.OrdinalIgnoreCase))
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF6060>Only the host can use this command.</color>");
                ClearChat(__instance);
                return false;
            }
    
            string targetName = textRegular.Substring(6).Trim();
            var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data?.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) == true);
    
            if (target == null)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    $"<color=#FF0000>Player \"{targetName}\" not found.</color>");
                ClearChat(__instance);
                return false;
            }
    
            if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>You cannot kick yourself.</color>");
                ClearChat(__instance);
                return false;
            }
    
            var clientId = GetClientId(target);
            if (clientId == -1)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>Could not find player.</color>");
                ClearChat(__instance);
                return false;
            }
    
            AmongUsClient.Instance.KickPlayer(clientId, false);
    
            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                $"Kicked <color=#FFA500>{target.Data.PlayerName}</color>.");
    
            ClearChat(__instance);
            return false;
        }

        // Adds /ban
        if (textRegular.StartsWith("/ban ", StringComparison.OrdinalIgnoreCase))
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>Only the host can use this command.</color>");
                ClearChat(__instance);
                return false;
            }
    
            string targetName = textRegular.Substring(5).Trim();
            var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data?.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) == true);
    
            if (target == null)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    $"<color=#FF0000>Player \"{targetName}\" not found.</color>");
                ClearChat(__instance);
                return false;
            }
    
            if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>You cannot ban yourself.</color>");
                ClearChat(__instance);
                return false;
            }
    
            var clientId = GetClientId(target);
            if (clientId == -1)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>Could not find player.</color>");
                ClearChat(__instance);
                return false;
            }
    
            AmongUsClient.Instance.KickPlayer(clientId, true);
    
            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                $"Banned <color=#FFA500>{target.Data.PlayerName}</color>.");
    
            ClearChat(__instance);
            return false;
        }

        if (spaceLess.StartsWith("/", StringComparison.OrdinalIgnoreCase)
            && summaryCommandList.Any(x => spaceLess.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            systemName = $"<color=#8BFDFD>{TouLocale.Get("EndGameSummary")}</color>";
            var title = systemName;
            var msg = TouLocale.GetParsed("SummaryMissingError");
            var summary = GameHistory.EndGameSummary;
            switch (LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.SummaryMessageAppearance.Value)
            {
                case GameSummaryAppearance.Advanced:
                    summary = GameHistory.EndGameSummaryAdvanced;
                    break;
                case GameSummaryAppearance.Simplified:
                    summary = GameHistory.EndGameSummarySimple;
                    break;
            }
            if (summary != string.Empty)
            {
                var factionText = string.Empty;
                if (GameHistory.WinningFaction != string.Empty)
                {
                    factionText =
                        $"<size=80%>{TouLocale.GetParsed("EndResult").Replace("<victoryType>", GameHistory.WinningFaction)}</size>\n";
                }

                title = $"{systemName}\n<size=62%>{factionText}{summary}</size>";
                msg = string.Empty;
            }

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        
        if (rulesCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                var stringToCheck =
                    rulesCommandList.FirstOrDefault(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase))!;
                var remainingText = textRegular;
                if (remainingText.StartsWith($"/{stringToCheck} ", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/{stringToCheck} ".Length..];
                }
                else if (remainingText.StartsWith($"/{stringToCheck}", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/{stringToCheck}".Length..];
                }
                else if (remainingText.StartsWith($"/ {stringToCheck} ", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/ {stringToCheck} ".Length..];
                }
                else if (remainingText.StartsWith($"/ {stringToCheck}", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/ {stringToCheck}".Length..];
                }

                if (remainingText.Trim().Equals("show", StringComparison.OrdinalIgnoreCase))
                {
                    var rulesText = GetLobbyRulesText();
                    RpcSendLobbyRulesGlobal(PlayerControl.LocalPlayer, rulesText);
                }
                else
                {
                    var rulesText = GetLobbyRulesText();
                    var title = $"<color=#8BFDFD>{TouLocale.GetParsed("RulesMessageTitle")}</color>";
                    var msg = string.IsNullOrWhiteSpace(rulesText) ? TouLocale.GetParsed("RulesMissingError") : $"<size=75%>{rulesText}</size>";
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);
                }
            }
            else
            {
                var stringToCheck =
                    rulesCommandList.FirstOrDefault(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase))!;
                var remainingText = textRegular;
                if (remainingText.StartsWith($"/{stringToCheck} ", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/{stringToCheck} ".Length..];
                }
                else if (remainingText.StartsWith($"/{stringToCheck}", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/{stringToCheck}".Length..];
                }
                else if (remainingText.StartsWith($"/ {stringToCheck} ", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/ {stringToCheck} ".Length..];
                }
                else if (remainingText.StartsWith($"/ {stringToCheck}", StringComparison.OrdinalIgnoreCase))
                {
                    remainingText = remainingText[$"/ {stringToCheck}".Length..];
                }

                if (remainingText.Trim().Equals("show", StringComparison.OrdinalIgnoreCase))
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("RulesShowHostError"));
                }
                else
                {
                    RpcRequestLobbyRules(PlayerControl.LocalPlayer);
                }
            }
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (nerfCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            var msg = TouLocale.GetParsed("NerfMeLobbyError");
            if (LobbyBehaviour.Instance)
            {
                VisionPatch.NerfMe = !VisionPatch.NerfMe;
                msg = TouLocale.GetParsed($"NerfMeToggle" + (VisionPatch.NerfMe ? "On" : "Off"));
            }

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (nameCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            var stringToCheck =
                nameCommandList.FirstOrDefault(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase))!;
            if (text.StartsWith($"/{stringToCheck} ", StringComparison.OrdinalIgnoreCase))
            {
                var charCount = $"/{stringToCheck} ".Length;
                textRegular = textRegular[charCount..];
            }
            else if (text.StartsWith($"/{stringToCheck}", StringComparison.OrdinalIgnoreCase))
            {
                var charCount = $"/{stringToCheck}".Length;
                textRegular = textRegular[charCount..];
            }
            else if (text.StartsWith($"/ {stringToCheck} ", StringComparison.OrdinalIgnoreCase))
            {
                var charCount = $"/ {stringToCheck} ".Length;
                textRegular = textRegular[charCount..];
            }
            else if (text.StartsWith($"/ {stringToCheck}", StringComparison.OrdinalIgnoreCase))
            {
                var charCount = $"/ {stringToCheck}".Length;
                textRegular = textRegular[charCount..];
            }

            var msg = TouLocale.GetParsed("SetNameLobbyError");
            if (LobbyBehaviour.Instance)
            {
                if (textRegular.Length < 1 || textRegular.Length > 12)
                {
                    msg = TouLocale.GetParsed("SetNameRequirementError");
                }
                else if (PlayerControl.AllPlayerControls.ToArray().Any(x =>
                             x.Data.PlayerName.ToLower(TownOfUsPlugin.Culture).Trim() ==
                             textRegular.ToLower(TownOfUsPlugin.Culture).Trim() &&
                             x.Data.PlayerId != PlayerControl.LocalPlayer.PlayerId))
                {
                    msg = TouLocale.GetParsed("SetNameSimilarError").Replace("<name>", textRegular);
                }
                else
                {
                    PlayerControl.LocalPlayer.CmdCheckName(textRegular);
                    msg = TouLocale.GetParsed("SetNameSuccess").Replace("<name>", textRegular);
                }
            }

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (upCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            if (AmongUsClient.Instance != null && !AmongUsClient.Instance.AmHost)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    TouLocale.GetParsed("UpCommandHostError"));
            }
            else if (!TownOfUsPlugin.IsDevBuild || TownOfUsPlugin.IsBetaBuild)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    TouLocale.GetParsed("UpCommandDevBuildError"));
            }
            else if (!LobbyBehaviour.Instance)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    TouLocale.GetParsed("UpCommandLobbyError"));
            }
            else
            {
                // Parse command: /up [Role] or /up [Role] [PlayerName]
                var commandMatch = upCommandList.FirstOrDefault(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase));
                if (commandMatch == null)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("UpCommandInvalidError"));
                }
                else
                {
                    var commandPrefix = $"/{commandMatch}";
                    var remainingText = textRegular;
                    if (remainingText.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        remainingText = remainingText[commandPrefix.Length..].TrimStart();
                    }

                    if (string.IsNullOrWhiteSpace(remainingText))
                    {
                        MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                            TouLocale.GetParsed("UpCommandNoRoleError"));
                    }
                    else
                    {
                        var parts = remainingText.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        var roleNameInput = parts[0];
                        string? targetPlayerName = null;

                        if (parts.Length > 1)
                        {
                            targetPlayerName = string.Join(" ", parts.Skip(1));
                        }

                        var allRoles = MiscUtils.SpawnableRoles.ToList();
                        var matchingRole = allRoles.FirstOrDefault(role =>
                            role.GetRoleName().Equals(roleNameInput, StringComparison.OrdinalIgnoreCase) ||
                            role.GetRoleName().Replace(" ", "").Equals(roleNameInput.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                            (role is ITownOfUsRole touRole && touRole.LocaleKey.Equals(roleNameInput, StringComparison.OrdinalIgnoreCase)));

                        if (matchingRole == null)
                        {
                            matchingRole = allRoles.FirstOrDefault(role =>
                                role.GetRoleName().Contains(roleNameInput, StringComparison.OrdinalIgnoreCase) ||
                                roleNameInput.Contains(role.GetRoleName(), StringComparison.OrdinalIgnoreCase) ||
                                (role is ITownOfUsRole touRole2 && (touRole2.LocaleKey.Contains(roleNameInput, StringComparison.OrdinalIgnoreCase) ||
                                                                    roleNameInput.Contains(touRole2.LocaleKey, StringComparison.OrdinalIgnoreCase))));
                        }

                        if (matchingRole == null)
                        {
                            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                TouLocale.GetParsed("UpCommandRoleNotFoundError").Replace("<role>", roleNameInput));
                        }
                        else
                        {
                            string targetName;
                            if (targetPlayerName != null)
                            {
                                var targetPlayer = PlayerControl.AllPlayerControls.ToArray()
                                    .FirstOrDefault(p => p.Data.PlayerName.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase) ||
                                                         p.Data.PlayerName.Contains(targetPlayerName, StringComparison.OrdinalIgnoreCase));

                                if (targetPlayer == null)
                                {
                                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                        TouLocale.GetParsed("UpCommandPlayerNotFoundError").Replace("<player>", targetPlayerName));
                                }
                                else
                                {
                                    targetName = targetPlayer.Data.PlayerName;
                                    var roleIdentifier = matchingRole is ITownOfUsRole touRole ? touRole.LocaleKey : matchingRole.GetRoleName();
                                    RpcForcePlayerRole(PlayerControl.LocalPlayer, targetPlayer);
                                    UpCommandRequests.SetRequest(targetName, roleIdentifier);
                                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                        TouLocale.GetParsed("UpCommandSuccessOther").Replace("<player>", targetName).Replace("<role>", MiscUtils.GetHyperlinkText(matchingRole)));
                                }
                            }
                            else
                            {
                                // Request for self
                                targetName = PlayerControl.LocalPlayer.Data.PlayerName;
                                var roleIdentifier = matchingRole is ITownOfUsRole touRole ? touRole.LocaleKey : matchingRole.GetRoleName();
                                RpcForcePlayerRole(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
                                UpCommandRequests.SetRequest(targetName, roleIdentifier);
                                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                    TouLocale.GetParsed("UpCommandSuccess").Replace("<role>", MiscUtils.GetHyperlinkText(matchingRole)));
                            }
                        }
                    }
                }
            }

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (rolesCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
            var roleOptions = currentGameOptions.RoleOptions;

            var allRoles = MiscUtils.AllRegisteredRoles.Where(role => !role.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(role) && roleOptions.GetNumPerGame(role.Role) > 0).OrderBy(x => x.GetRoleName()).ToList();
            var ghostRoles = MiscUtils.GetRegisteredGhostRoles().Where(role => CustomRoleUtils.CanSpawnOnCurrentMode(role) && roleOptions.GetNumPerGame(role.Role) > 0).OrderBy(x => x.GetRoleName()).ToList();

            var crewmateRoles = new List<RoleBehaviour>();
            var impostorRoles = new List<RoleBehaviour>();
            var neutralRoles = new List<RoleBehaviour>();
            var allGhostRoles = new List<RoleBehaviour>();

            foreach (var role in allRoles)
            {
                var alignment = role.GetRoleAlignment();
                if (alignment.ToString().Contains("Crewmate"))
                {
                    crewmateRoles.Add(role);
                }
                else if (alignment.ToString().Contains("Impostor"))
                {
                    impostorRoles.Add(role);
                }
                else
                {
                    neutralRoles.Add(role);
                }
            }

            foreach (var role in ghostRoles)
            {
                if (role is ICustomRole custom && custom.Configuration.HideSettings)
                {
                    continue;
                }

                allGhostRoles.Add(role);
            }

            var roleNameToLink = new Func<RoleBehaviour, string>(role =>
            {
                return MiscUtils.GetHyperlinkText(role);
            });

            var msgParts = new List<string>();

            var rolesHeader = TouLocale.GetParsed("RolesHeader");
            var crewWord = TouLocale.GetParsed("CrewmateKeyword");
            var impWord = TouLocale.GetParsed("ImpostorKeyword");
            var neutWord = TouLocale.GetParsed("NeutralKeyword");
            var ghostWord = TouLocale.GetParsed("GhostKeyword");
            if (crewmateRoles.Count > 0)
            {
                msgParts.Add($"{rolesHeader.Replace("<type>", crewWord)} ({crewmateRoles.Count}):\n{string.Join(", ", crewmateRoles.Select(roleNameToLink))}");
            }

            if (impostorRoles.Count > 0)
            {
                msgParts.Add($"{rolesHeader.Replace("<type>", impWord)} ({impostorRoles.Count}):\n{string.Join(", ", impostorRoles.Select(roleNameToLink))}");
            }

            if (neutralRoles.Count > 0)
            {
                msgParts.Add($"{rolesHeader.Replace("<type>", neutWord)} ({neutralRoles.Count}):\n{string.Join(", ", neutralRoles.Select(roleNameToLink))}");
            }

            if (allGhostRoles.Count > 0)
            {
                msgParts.Add($"{rolesHeader.Replace("<type>", ghostWord)} ({allGhostRoles.Count}):\n{string.Join(", ", allGhostRoles.Select(roleNameToLink))}");
            }

            var msg = string.Join("\n\n", msgParts);

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (helpCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            List<string> randomNames =
            [
                "Atony", "Alchlc", "angxlwtf", "Digi", "Donners", "K3ndo", "DragonBreath", "Pietro", "Nix", "Daemon",
                "6pak", "Chipseq", "satire", "Sarha", "vanpla", "neil",
                "twix", "xerm", "XtraCube", "Zeo", "Slushie", "chloe", "moon", "decii", "Northie", "GD", "Chilled",
                "Himi", "Riki", "Leafly", "miniduikboot"
            ];

            var msg = $"<size=75%>{TouLocale.GetParsed("HelpMessageTitle")}\n" +
                      $"{TouLocale.GetParsed("HelpCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("NerfMeCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("SetNameCommandDescription").Replace("<randomName>", randomNames.Random())}\n" +
                      $"{TouLocale.GetParsed("SpectateCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("RolesCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("SummaryCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("RulesCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("InfoCommandDescription")}\n";

            // Only show /up command in help if host + dev build (not beta)
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && TownOfUsPlugin.IsDevBuild && !TownOfUsPlugin.IsBetaBuild)
            {
                msg += $"{TouLocale.GetParsed("UpCommandDescription")}\n";
            }

            msg += "</size>";

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, msg);

            if (ModCompatibility.CommandModsInstalled)
            {
                return true;
            }
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        // /info — replays all FakeChat messages received this meeting
        if (infoCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            if (FakeChatHistory.HasInfo)
            {
                FakeChatHistory.IsReplaying = true;
                foreach (var (infoTitle, infoMsg) in FakeChatHistory.GetEntries())
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, infoTitle, infoMsg, false, true);
                }
                FakeChatHistory.IsReplaying = false;
            }
            else
            {
                MiscUtils.AddSystemChat(
                    PlayerControl.LocalPlayer.Data,
                    systemName,
                    TouLocale.GetParsed("InfoCommandNoInfo"));
            }

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (spaceLess.StartsWith("/jail", StringComparison.OrdinalIgnoreCase))
        {
            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, TouLocale.GetParsed("JailCommandError"));

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (TeamChatPatches.TeamChatActive && !PlayerControl.LocalPlayer.HasDied() && TeamChatPatches.TeamChatManager.SendMessage(textRegular))
        {
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();

            return false;
        }

        // Chat History
        if (textRegular.Length > 0)
        {
            if (ChatControllerPatches.ChatHistory.Count == 0 || ChatControllerPatches.ChatHistory[^1] != textRegular)
            {
                ChatControllerPatches.ChatHistory.Add(textRegular);
                if (ChatControllerPatches.ChatHistory.Count > 20)
                {
                    ChatControllerPatches.ChatHistory.RemoveAt(0);
                }
            }
            ChatControllerPatches.CurrentHistorySelection = ChatControllerPatches.ChatHistory.Count;
        }

        return true;
    }


    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static bool Prefix(ChatController __instance)
    {
        var text = __instance.freeChatField.Text.ToLower(TownOfUsPlugin.Culture);
        var textRegular = __instance.freeChatField.Text.WithoutRichText();

        // Remove chat limit
        if (textRegular.Length < 1)
        {
            return true;
        }

        var systemName = $"<color=#8BFDFD>{TouLocale.GetParsed("SystemChatTitle")}</color>";
        var spaceLess = text.Replace(" ", string.Empty);

        if (spaceLess.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                TouLocale.GetParsed("NoCommandFoundError"));

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.RequestLobbyRules)]
    public static void RpcRequestLobbyRules(PlayerControl requester)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        var rulesText = GetLobbyRulesText();
        RpcSendLobbyRules(PlayerControl.LocalPlayer, requester, rulesText, false);
    }

    [MethodRpc((uint)TownOfUsRpc.SendLobbyRules)]
    public static void RpcSendLobbyRules(PlayerControl host, PlayerControl target, string rulesText, bool optional)
    {
        if (!host.IsHost())
        {
            MiscUtils.RunAnticheatWarning(host);
            return;
        }
        if (PlayerControl.LocalPlayer.PlayerId != target.PlayerId || optional && !LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.ShowRulesOnLobbyJoinToggle.Value)
        {
            return;
        }
        var title = $"<color=#8BFDFD>{TouLocale.GetParsed("RulesMessageTitle")}</color>";
        var msg = string.IsNullOrWhiteSpace(rulesText) ? TouLocale.GetParsed("RulesMissingError") : $"<size=75%>{rulesText}</size>";
        MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);
    }

    [MethodRpc((uint)TownOfUsRpc.SendLobbyRulesGlobal)]
    public static void RpcSendLobbyRulesGlobal(PlayerControl host, string rulesText)
    {
        if (!host.IsHost())
        {
            MiscUtils.RunAnticheatWarning(host);
            return;
        }
        var title = $"<color=#8BFDFD>{TouLocale.GetParsed("RulesMessageTitle")}</color>";
        var msg = string.IsNullOrWhiteSpace(rulesText) ? TouLocale.GetParsed("RulesMissingError") : $"<size=75%>{rulesText}</size>";
        MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);
    }

    [MethodRpc((uint)TownOfUsRpc.SelectSpectator)]
    public static void RpcSelectSpectator(PlayerControl player)
    {
        if (!OptionGroupSingleton<HostSpecificOptions>.Instance.EnableSpectators.Value)
        {
            return;
        }

        if (!SpectatorRole.TrackedSpectators.Contains(player.Data.PlayerName))
        {
            SpectatorRole.TrackedSpectators.Add(player.Data.PlayerName);
        }
    }

    public static void SetSpectatorList(Dictionary<byte, string> list)
    {
        SpectatorRole.TrackedSpectators.Clear();

        foreach (var name in list.Select(x => x.Value))
        {
            SpectatorRole.TrackedSpectators.Add(name);
        }
    }

    public static void ClearSpectatorList()
    {
        SpectatorRole.TrackedSpectators.Clear();
    }

    [MethodRpc((uint)TownOfUsRpc.RemoveSpectator)]
    public static void RpcRemoveSpectator(PlayerControl player)
    {
        if (SpectatorRole.TrackedSpectators.Contains(player.Data.PlayerName))
        {
            SpectatorRole.TrackedSpectators.Remove(player.Data.PlayerName);
        }
    }

    private static void ClearChat(ChatController chat)
    {
        chat.freeChatField.Clear();
        chat.quickChatMenu.Clear();
        chat.quickChatField.Clear();
        chat.UpdateChatMode();
    }

    private static int GetClientId(PlayerControl player)
    {
        foreach (var client in AmongUsClient.Instance.allClients.ToArray())
        {
            try
            {
                var charProp = client.GetType().GetProperty("Character") ?? client.GetType().GetProperty("character");
                if (charProp?.GetValue(client) is PlayerControl pc && pc.PlayerId == player.PlayerId)
                {
                    var idProp = client.GetType().GetProperty("Id") ?? client.GetType().GetProperty("id");
                    if (idProp?.GetValue(client) is int id)
                        return id;
                }
            }
            catch { }
        }
        return -1;
    }
}
