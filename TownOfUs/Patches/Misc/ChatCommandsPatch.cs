using System.Collections;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.DraftMode;
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

        var systemName = $"<color=#8BFDFD>{MiraLocaleManager.Get("SystemChatTitle")}</color>";
        MiscUtils.AddSystemChat(host.Data, systemName,
            MiraLocaleManager.Get("UpCommandSuccessGlobal").Replace("<player>", player.Data.PlayerName));
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

        var systemName = $"<color=#8BFDFD>{MiraLocaleManager.Get("SystemChatTitle")}</color>";
        var specCommandList = MiraLocaleManager.Get("SpectatorCommandList").Split(":");
        var summaryCommandList = MiraLocaleManager.Get("SummaryCommandList").Split(":");
        var rolesCommandList = MiraLocaleManager.Get("RolesCommandList").Split(":");
        var nerfCommandList = MiraLocaleManager.Get("NerfMeCommandList").Split(":");
        var playerCommandList = MiraLocaleManager.Get("PlayerCommandList").Split(":");
        var nameCommandList = MiraLocaleManager.Get("SetNameCommandList").Split(":");
        var helpCommandList = MiraLocaleManager.Get("HelpCommandList").Split(":");
        var upCommandList = MiraLocaleManager.Get("UpCommandList").Split(":");
        var rulesCommandList = MiraLocaleManager.Get("RulesCommandList").Split(":");
        var infoCommandList = MiraLocaleManager.Get("InfoCommandList").Split(":");

        if (MiraLocaleManager.CurrentLanguage is not MiraLanguage.English)
        {
            specCommandList = specCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "SpectatorCommandList").Split(":"));
            summaryCommandList = summaryCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "SummaryCommandList").Split(":"));
            rolesCommandList = rolesCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "RolesCommandList").Split(":"));
            nerfCommandList = nerfCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "NerfMeCommandList").Split(":"));
            playerCommandList = playerCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "PlayerCommandList").Split(":"));
            nameCommandList = nameCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "SetNameCommandList").Split(":"));
            helpCommandList = helpCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "HelpCommandList").Split(":"));
            upCommandList = upCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "UpCommandList").Split(":"));
            rulesCommandList = rulesCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "RulesCommandList").Split(":"));
            infoCommandList = infoCommandList.AddRangeToArray(MiraLocaleManager.Get(MiraLanguage.English, "InfoCommandList").Split(":"));
        }

        var spaceLess = text.Replace(" ", string.Empty);
        if (specCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            if (!LobbyBehaviour.Instance)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    MiraLocaleManager.Get("SpectatorLobbyError"));
            }
            else
            {
                if (GameStartManager.InstanceExists &&
                    GameStartManager.Instance.startState is GameStartManager.StartingStates.Countdown)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        MiraLocaleManager.Get("SpectatorStartError"));
                }
                else if (DraftManager.IsDraftActive)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        MiraLocaleManager.Get("SpectatorDraftError"));
                }
                else if (SpectatorRole.TrackedSpectators.Contains(PlayerControl.LocalPlayer.Data.PlayerName))
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        MiraLocaleManager.Get("SpectatorToggleOff"));
                    RpcRemoveSpectator(PlayerControl.LocalPlayer);
                }
                else if (!OptionGroupSingleton<HostSpecificOptions>.Instance.EnableSpectators)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        MiraLocaleManager.Get("SpectatorHostError"));
                }
                else
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        MiraLocaleManager.Get("SpectatorToggleOn"));
                    RpcSelectSpectator(PlayerControl.LocalPlayer);
                }
            }

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
    
            string targetName = textRegular[6..].Trim();
            var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data?.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) == true);
    
            if (target == null)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    $"<color=#FF0000>Player \"{targetName}\" not found.</color>");
                ClearChat(__instance);
                return false;
            }
    
            if (target.AmOwner)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>You cannot kick yourself.</color>");
                ClearChat(__instance);
                return false;
            }
    
            var clientId = target.OwnerId;
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
    
            string targetName = textRegular[5..].Trim();
            var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data?.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) == true);
    
            if (target == null)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    $"<color=#FF0000>Player \"{targetName}\" not found.</color>");
                ClearChat(__instance);
                return false;
            }
    
            if (target.AmOwner)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    "<color=#FF0000>You cannot ban yourself.</color>");
                ClearChat(__instance);
                return false;
            }

            var clientId = target.OwnerId;
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
            systemName = $"<color=#8BFDFD>{MiraLocaleManager.Get("EndGameSummary")}</color>";
            var title = systemName;
            var msg = MiraLocaleManager.Get("SummaryMissingError");
            var summary = GameHistory.EndGameSummary;
            switch (LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.SummaryMessageAppearance.Value)
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
                        $"<size=80%>{MiraLocaleManager.Get("EndResult").Replace("<victoryType>", GameHistory.WinningFaction)}</size>\n";
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
            if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
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
                    var title = $"<color=#8BFDFD>{MiraLocaleManager.Get("RulesMessageTitle")}</color>";
                    var msg = string.IsNullOrWhiteSpace(rulesText) ? MiraLocaleManager.Get("RulesMissingError") : $"<size=75%>{rulesText}</size>";
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
                        MiraLocaleManager.Get("RulesShowHostError"));
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
            var msg = MiraLocaleManager.Get("NerfMeLobbyError");
            if (LobbyBehaviour.Instance)
            {
                VisionPatch.NerfMe = !VisionPatch.NerfMe;
                msg = MiraLocaleManager.Get($"NerfMeToggle" + (VisionPatch.NerfMe ? "On" : "Off"));
            }

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (playerCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            var sBuilder = new StringBuilder();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (HudManagerHelper.PlatformAssociations.TryGetValue(AmongUsClient.Instance.GetClientFromCharacter(player).Id, out var icon))
                {
                    sBuilder.AppendLine(MiraLocaleManager.Get("PlayerCommandDetails")
                        .Replace("<player>", player.CachedPlayerData.PlayerName).Replace("<platform>", icon));
                }
                else
                {
                    sBuilder.AppendLine(MiraLocaleManager.Get("PlayerCommandDetails")
                        .Replace("<player>", player.CachedPlayerData.PlayerName).Replace("<platform>", "<sprite name=\"Platform.Blank\">"));
                }
            }

            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, sBuilder.ToString());

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

            var msg = MiraLocaleManager.Get("SetNameLobbyError");
            if (LobbyBehaviour.Instance)
            {
                if (textRegular.Length < 1 || textRegular.Length > 12)
                {
                    msg = MiraLocaleManager.Get("SetNameRequirementError");
                }
                else if (PlayerControl.AllPlayerControls.ToArray().Any(x =>
                             x.Data.PlayerName.ToLower(TownOfUsPlugin.Culture).Trim() ==
                             textRegular.ToLower(TownOfUsPlugin.Culture).Trim() &&
                             !x.AmOwner))
                {
                    msg = MiraLocaleManager.Get("SetNameSimilarError").Replace("<name>", textRegular);
                }
                else
                {
                    PlayerControl.LocalPlayer.CmdCheckName(textRegular);
                    msg = MiraLocaleManager.Get("SetNameSuccess").Replace("<name>", textRegular);
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
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    MiraLocaleManager.Get("UpCommandHostError"));
            }
            else if (!TownOfUsPlugin.IsDevBuild || TownOfUsPlugin.IsBetaBuild)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    MiraLocaleManager.Get("UpCommandDevBuildError"));
            }
            else if (!LobbyBehaviour.Instance)
            {
                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                    MiraLocaleManager.Get("UpCommandLobbyError"));
            }
            else
            {
                // Parse command: /up [Role] or /up [Role] [PlayerName]
                var commandMatch = upCommandList.FirstOrDefault(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase));
                if (commandMatch == null)
                {
                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                        MiraLocaleManager.Get("UpCommandInvalidError"));
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
                            MiraLocaleManager.Get("UpCommandNoRoleError"));
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
                        var matchingRole =
                            allRoles.FirstOrDefault(role =>
                                role.GetRoleName().Equals(roleNameInput, StringComparison.OrdinalIgnoreCase) ||
                                role.GetRoleName().Replace(" ", "").Equals(roleNameInput.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                                (role is ITownOfUsRole touRole && touRole.IdPart.Equals(roleNameInput, StringComparison.OrdinalIgnoreCase)))
                            ?? allRoles.FirstOrDefault(role =>
                                role.GetRoleName().Contains(roleNameInput, StringComparison.OrdinalIgnoreCase) ||
                                roleNameInput.Contains(role.GetRoleName(), StringComparison.OrdinalIgnoreCase) ||
                                (role is ITownOfUsRole touRole2 && (touRole2.IdPart.Contains(roleNameInput, StringComparison.OrdinalIgnoreCase) ||
                                                                    roleNameInput.Contains(touRole2.IdPart, StringComparison.OrdinalIgnoreCase))));
                        if (matchingRole == null)
                        {
                            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                MiraLocaleManager.Get("UpCommandRoleNotFoundError").Replace("<role>", roleNameInput));
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
                                        MiraLocaleManager.Get("UpCommandPlayerNotFoundError").Replace("<player>", targetPlayerName));
                                }
                                else
                                {
                                    targetName = targetPlayer.Data.PlayerName;
                                    var roleIdentifier = matchingRole is ITownOfUsRole touRole ? touRole.IdPart : matchingRole.GetRoleName();
                                    RpcForcePlayerRole(PlayerControl.LocalPlayer, targetPlayer);
                                    UpCommandRequests.SetRequest(targetName, roleIdentifier);
                                    MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                        MiraLocaleManager.Get("UpCommandSuccessOther").Replace("<player>", targetName).Replace("<role>", MiscUtils.GetHyperlinkText(matchingRole)));
                                }
                            }
                            else
                            {
                                // Request for self
                                targetName = PlayerControl.LocalPlayer.Data.PlayerName;
                                var roleIdentifier = matchingRole is ITownOfUsRole touRole ? touRole.IdPart : matchingRole.GetRoleName();
                                RpcForcePlayerRole(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
                                UpCommandRequests.SetRequest(targetName, roleIdentifier);
                                MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                                    MiraLocaleManager.Get("UpCommandSuccess").Replace("<role>", MiscUtils.GetHyperlinkText(matchingRole)));
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

            var rolesHeader = MiraLocaleManager.Get("RolesHeader");
            var crewWord = MiraLocaleManager.Get("CrewmateKeyword");
            var impWord = MiraLocaleManager.Get("ImpostorKeyword");
            var neutWord = MiraLocaleManager.Get("NeutralKeyword");
            var ghostWord = MiraLocaleManager.Get("GhostKeyword");
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

            var msg = $"<size=75%>{MiraLocaleManager.Get("HelpMessageTitle")}\n" +
                      $"{MiraLocaleManager.Get("HelpCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("NerfMeCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("PlayerCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("SetNameCommandDescription").Replace("<randomName>", randomNames.Random())}\n" +
                      $"{MiraLocaleManager.Get("SpectateCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("RolesCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("SummaryCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("RulesCommandDescription")}\n" +
                      $"{MiraLocaleManager.Get("InfoCommandDescription")}\n";

            // Only show /up command in help if host + dev build (not beta)
            if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost && TownOfUsPlugin.IsDevBuild && !TownOfUsPlugin.IsBetaBuild)
            {
                msg += $"{MiraLocaleManager.Get("UpCommandDescription")}\n";
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
                    MiraLocaleManager.Get("InfoCommandNoInfo"));
            }

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (spaceLess.StartsWith("/jail", StringComparison.OrdinalIgnoreCase))
        {
            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName, MiraLocaleManager.Get("JailCommandError"));

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

        var systemName = $"<color=#8BFDFD>{MiraLocaleManager.Get("SystemChatTitle")}</color>";
        var spaceLess = text.Replace(" ", string.Empty);

        if (spaceLess.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, systemName,
                MiraLocaleManager.Get("NoCommandFoundError"));

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.RequestLobbyRules)]
    private static void RpcRequestLobbyRules(PlayerControl requester)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        var rulesText = GetLobbyRulesText();
        RpcSendLobbyRules(PlayerControl.LocalPlayer, requester, rulesText, false);
    }

    private static bool _canShowRules = true;
    [MethodRpc((uint)TownOfUsRpc.SendLobbyRules)]
    internal static void RpcSendLobbyRules(PlayerControl host, PlayerControl target, string rulesText, bool optional)
    {
        if (!host.IsHost())
        {
            MiscUtils.RunAnticheatWarning(host);
            return;
        }
        if (!_canShowRules)
        {
            return;
        }
        if (PlayerControl.LocalPlayer.PlayerId != target.PlayerId || optional && !LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ShowRulesOnLobbyJoinToggle.Value)
        {
            return;
        }
        var title = $"<color=#8BFDFD>{MiraLocaleManager.Get("RulesMessageTitle")}</color>";
        var msg = string.IsNullOrWhiteSpace(rulesText) ? MiraLocaleManager.Get("RulesMissingError") : $"<size=75%>{rulesText}</size>";
        MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);
        Coroutines.Start(CoWaitForAcCooldown());
    }

    private static IEnumerator CoWaitForAcCooldown()
    {
        var acWarnTimer = 3f;
        _canShowRules = false;
        while (acWarnTimer > 0)
        {
            acWarnTimer -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        _canShowRules = true;
    }

    [MethodRpc((uint)TownOfUsRpc.SendLobbyRulesGlobal)]
    private static void RpcSendLobbyRulesGlobal(PlayerControl host, string rulesText)
    {
        if (!host.IsHost())
        {
            MiscUtils.RunAnticheatWarning(host);
            return;
        }
        if (!_canShowRules)
        {
            return;
        }
        var title = $"<color=#8BFDFD>{MiraLocaleManager.Get("RulesMessageTitle")}</color>";
        var msg = string.IsNullOrWhiteSpace(rulesText) ? MiraLocaleManager.Get("RulesMissingError") : $"<size=75%>{rulesText}</size>";
        MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data, title, msg);
        Coroutines.Start(CoWaitForAcCooldown());
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
}
