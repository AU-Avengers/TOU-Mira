using AmongUs.GameOptions;
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
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatPatches
{
    // ReSharper disable once InconsistentNaming
    public static bool Prefix(ChatController __instance)
    {
        var text = __instance.freeChatField.Text.ToLower(TownOfUsPlugin.Culture);
        var textRegular = __instance.freeChatField.Text.WithoutRichText();

        // Remove chat delay if host
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            __instance.timeSinceLastMessage = 999f;
        }

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

        if (TranslationController.InstanceExists &&
            TranslationController.Instance.currentLanguage.languageID is not SupportedLangs.English)
        {
            specCommandList = specCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "SpectatorCommandList").Split(":"));
            summaryCommandList = summaryCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "SummaryCommandList").Split(":"));
            rolesCommandList = rolesCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "RolesCommandList").Split(":"));
            nerfCommandList = nerfCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "NerfMeCommandList").Split(":"));
            nameCommandList = nameCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "SetNameCommandList").Split(":"));
            helpCommandList = helpCommandList.AddRangeToArray(TouLocale.GetParsed(SupportedLangs.English, "HelpCommandList").Split(":"));
        }

        var spaceLess = text.Replace(" ", string.Empty);
        if (specCommandList.Any(x => spaceLess.StartsWith($"/{x}", StringComparison.OrdinalIgnoreCase)))
        {
            if (!LobbyBehaviour.Instance)
            {
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName,
                    TouLocale.GetParsed("SpectatorLobbyError"));
            }
            else
            {
                if (GameStartManager.InstanceExists &&
                    GameStartManager.Instance.startState is GameStartManager.StartingStates.Countdown)
                {
                    MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorStartError"));
                }
                else if (SpectatorRole.TrackedSpectators.Contains(PlayerControl.LocalPlayer.Data.PlayerName))
                {
                    MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorToggleOff"));
                    RpcRemoveSpectator(PlayerControl.LocalPlayer);
                }
                else if (!OptionGroupSingleton<HostSpecificOptions>.Instance.EnableSpectators)
                {
                    MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName,
                        TouLocale.GetParsed("SpectatorHostError"));
                }
                else
                {
                    MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName,
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
            if (GameHistory.EndGameSummary != string.Empty)
            {
                var factionText = string.Empty;
                if (GameHistory.WinningFaction != string.Empty)
                {
                    factionText =
                        $"<size=80%>{TouLocale.GetParsed("EndResult").Replace("<victoryType>", GameHistory.WinningFaction)}</size>\n";
                }

                title = $"{systemName}\n<size=62%>{factionText}{GameHistory.EndGameSummary}</size>";
                msg = string.Empty;
            }

            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);

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

            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName, msg);

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

            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName, msg);

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
            var allRoles = MiscUtils.AllRegisteredRoles.Where(role => !role.IsDead && roleOptions.GetNumPerGame(role.Role) > 0).ToList();
            var ghostRoles = MiscUtils.GetRegisteredGhostRoles().Where(role => roleOptions.GetNumPerGame(role.Role) > 0).ToList();

            var crewmateRoles = new List<RoleBehaviour>();
            var impostorRoles = new List<RoleBehaviour>();
            var neutralRoles = new List<RoleBehaviour>();
            var neutralGhostRoles = new List<RoleBehaviour>();

            foreach (var role in allRoles)
            {
                var alignment = role.GetRoleAlignment();
                if (alignment is RoleAlignment.CrewmateInvestigative or RoleAlignment.CrewmateKilling or
                    RoleAlignment.CrewmateProtective or RoleAlignment.CrewmatePower or RoleAlignment.CrewmateSupport)
                {
                    crewmateRoles.Add(role);
                }
                else if (alignment is RoleAlignment.ImpostorConcealing or RoleAlignment.ImpostorKilling or
                         RoleAlignment.ImpostorPower or RoleAlignment.ImpostorSupport)
                {
                    impostorRoles.Add(role);
                }
                else if (alignment is RoleAlignment.NeutralBenign or RoleAlignment.NeutralEvil or
                         RoleAlignment.NeutralKilling or RoleAlignment.NeutralOutlier)
                {
                    neutralRoles.Add(role);
                }
            }

            var neutralGhostRoleId = (RoleTypes)RoleId.Get<NeutralGhostRole>();
            foreach (var role in ghostRoles)
            {
                if (role.Role == neutralGhostRoleId)
                {
                    continue;
                }

                neutralGhostRoles.Add(role);
            }

            var roleNameToLink = new Func<RoleBehaviour, string>(role =>
            {
                var roleName = role.GetRoleName();
                return $"#{roleName.Replace(" ", "-")}";
            });

            var msgParts = new List<string>();

            // Idk if I should be using localization for the titles here... :/
            if (crewmateRoles.Count > 0)
            {
                msgParts.Add($"Crewmate Roles ({crewmateRoles.Count}):\n{string.Join(", ", crewmateRoles.Select(roleNameToLink))}");
            }

            if (impostorRoles.Count > 0)
            {
                msgParts.Add($"Imposter Roles ({impostorRoles.Count}):\n{string.Join(", ", impostorRoles.Select(roleNameToLink))}");
            }

            if (neutralRoles.Count > 0)
            {
                msgParts.Add($"Neutral Roles ({neutralRoles.Count}):\n{string.Join(", ", neutralRoles.Select(roleNameToLink))}");
            }

            if (neutralGhostRoles.Count > 0)
            {
                msgParts.Add($"Ghost Roles ({neutralGhostRoles.Count}):\n{string.Join(", ", neutralGhostRoles.Select(roleNameToLink))}");
            }

            var msg = string.Join("\n\n", msgParts);

            // Send as regular chat message so everyone can see it
            // Use vanilla RPC to send to all players
            //var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.Reliable, -1);
            //writer.Write(msg);
            //AmongUsClient.Instance.FinishRpcImmediately(writer);

            //// Should probably use FakeChat in production but uhhh yeah hacky haky

            //// Also add locally
            //if (HudManager.InstanceExists)
            //{
            //    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
            //}

            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName, msg);

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
                "6pak", "Chipseq",
                "twix", "xerm", "XtraCube", "Zeo", "Slushie", "chloe", "moon", "decii", "Northie", "GD", "Chilled",
                "Himi", "Riki", "Leafly", "miniduikboot"
            ];

            var msg = $"<size=75%>{TouLocale.GetParsed("HelpMessageTitle")}\n" +
                      $"{TouLocale.GetParsed("HelpCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("NerfMeCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("SetNameCommandDescription").Replace("<randomName>", randomNames.Random())}\n" +
                      $"{TouLocale.GetParsed("SpectateCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("RolesCommandDescription")}\n" +
                      $"{TouLocale.GetParsed("SummaryCommandDescription")}\n</size>";

            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (spaceLess.StartsWith("/jail", StringComparison.OrdinalIgnoreCase))
        {
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName, TouLocale.GetParsed("JailCommandError"));

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (spaceLess.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, systemName,
                TouLocale.GetParsed("NoCommandFoundError"));

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }

        if (TeamChatPatches.TeamChatActive && !PlayerControl.LocalPlayer.HasDied() &&
            (PlayerControl.LocalPlayer.Data.Role is JailorRole || PlayerControl.LocalPlayer.IsJailed() ||
             PlayerControl.LocalPlayer.Data.Role is VampireRole || PlayerControl.LocalPlayer.IsImpostorAligned()))
        {
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

            if (PlayerControl.LocalPlayer.IsImpostorAligned() &&
                genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true })
            {
                TeamChatPatches.RpcSendImpTeamChat(PlayerControl.LocalPlayer, textRegular);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }

            if (PlayerControl.LocalPlayer.Data.Role is JailorRole)
            {
                TeamChatPatches.RpcSendJailorChat(PlayerControl.LocalPlayer, textRegular);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }

            if (PlayerControl.LocalPlayer.IsJailed())
            {
                TeamChatPatches.RpcSendJaileeChat(PlayerControl.LocalPlayer, textRegular);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }

            if (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat)
            {
                TeamChatPatches.RpcSendVampTeamChat(PlayerControl.LocalPlayer, textRegular);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }
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
        var oldList = SpectatorRole.TrackedSpectators;
        foreach (var name in oldList)
        {
            SpectatorRole.TrackedSpectators.Remove(name);
        }

        foreach (var name in list.Select(x => x.Value))
        {
            SpectatorRole.TrackedSpectators.Add(name);
        }
    }

    public static void ClearSpectatorList()
    {
        var oldList = SpectatorRole.TrackedSpectators;
        foreach (var name in oldList)
        {
            SpectatorRole.TrackedSpectators.Remove(name);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.RemoveSpectator)]
    public static void RpcRemoveSpectator(PlayerControl player)
    {
        if (SpectatorRole.TrackedSpectators.Contains(player.Data.PlayerName))
        {
            SpectatorRole.TrackedSpectators.Remove(player.Data.PlayerName);
        }
    }
}
