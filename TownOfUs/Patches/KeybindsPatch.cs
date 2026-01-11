using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Rewired;
using TownOfUs.Buttons;
using TownOfUs.Events.Modifiers;
using TownOfUs.GameOver;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Bindings
{
    private static int? _originalPlayerLayer;
    private static bool _wasCtrlHeld;

    public static void Postfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data == null)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (TutorialManager.InstanceExists && Input.GetKeyDown(KeyCode.F9))
        {
            FreeplayButtonsVisibility.Toggle();
        }

        var isHost = PlayerControl.LocalPlayer.IsHost();

        //  Full List of binds:
        //      Suicide Keybind (ENTER + T + Left Shift)
        //      End Game Keybind (ENTER + L + Left Shift)
        //      Start Meeting (ENTER + K + Left Shift)
        //      End Meeting Keybind (F6)
        //      Random Impostor Role (F3)
        //      Random Neutral Killer Role (F4)
        //      CTRL to pass through objects in lobby ONLY
        if (isHost) // Disable all keybinds except CTRL in lobby if not host (NOTE: Might want a toggle in settings for these binds?)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
            {
                // Suicide Keybind (ENTER + T + Left Shift)
                if (!PlayerControl.LocalPlayer.HasDied() && Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.T) && Input.GetKey(KeyCode.LeftShift))
                {
                    PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
                }

                // End Game Keybind (ENTER + L + Left Shift)
                if (Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.LeftShift))
                {
                    var gameFlow = GameManager.Instance.LogicFlow.Cast<LogicGameFlowNormal>();
                    if (gameFlow != null)
                    {
                        CustomGameOver.Trigger<HostGameOver>([]);
                    }
                }

                // Start Meeting (ENTER + K + Left Shift)
                if (!MeetingHud.Instance &&
                    !ExileController.Instance && Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.K) &&
                    Input.GetKey(KeyCode.LeftShift))
                {
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                    if (!GameManager.Instance.CheckTaskCompletion())
                    {
                        HudManager.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                        PlayerControl.LocalPlayer.RpcStartMeeting(null);
                    }
                }
            }

            // End Meeting Keybind (F6)
            if (Input.GetKeyDown(KeyCode.F6) && MeetingHud.Instance)
            {
                var hud = MeetingHud.Instance;

                var areas = hud.playerStates;
                foreach (var area in areas)
                {
                    if (area.VotedFor != byte.MaxValue && area.VotedFor != area.TargetPlayerId)
                    {
                        var voter = MiscUtils.PlayerById(area.TargetPlayerId);
                        if (voter != null && !voter.HasDied())
                        {
                            var voteData = voter.GetVoteData();
                            if (!voteData.Votes.Any(v => v.Voter == area.TargetPlayerId && v.Suspect == area.VotedFor))
                            {
                                voteData.VoteForPlayer(area.VotedFor);
                            }
                        }
                    }
                }

                MiraEventManager.InvokeEvent(new CheckForEndVotingEvent(true));

                var finalVoteList = new List<CustomVote>();
                foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                {
                    if (player == null || player.HasDied())
                    {
                        continue;
                    }

                    var voteData = player.GetVoteData();
                    foreach (var vote in voteData.Votes)
                    {
                        finalVoteList.Add(vote);
                    }
                }

                var seededExiled = VotingUtils.GetExiled(finalVoteList, out var seededTie);
                if (seededTie)
                {
                    seededExiled = null;
                }

                var processEvent = new ProcessVotesEvent(finalVoteList)
                {
                    ExiledPlayer = seededExiled
                };
                MiraEventManager.InvokeEvent(processEvent);

                var votesForStates = processEvent.Votes.ToList();
                if (TiebreakerEvents.TiebreakingVote.HasValue)
                {
                    votesForStates.Add(TiebreakerEvents.TiebreakingVote.Value);
                }

                var playerIdsWithAnyVote = new HashSet<byte>();
                var voterStatesList = new List<MeetingHud.VoterState>();
                foreach (var vote in votesForStates)
                {
                    playerIdsWithAnyVote.Add(vote.Voter);
                    voterStatesList.Add(new MeetingHud.VoterState
                    {
                        VoterId = vote.Voter,
                        VotedForId = vote.Suspect
                    });
                }

                foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                {
                    if (player == null || player.HasDied())
                    {
                        continue;
                    }

                    if (playerIdsWithAnyVote.Contains(player.PlayerId))
                    {
                        continue;
                    }

                    voterStatesList.Add(new MeetingHud.VoterState
                    {
                        VoterId = player.PlayerId,
                        VotedForId = byte.MaxValue
                    });
                }

                var voterStates = new Il2CppStructArray<MeetingHud.VoterState>(voterStatesList.Count);
                for (int i = 0; i < voterStatesList.Count; i++)
                {
                    voterStates[i] = voterStatesList[i];
                }

                var exiled = processEvent.ExiledPlayer;
                bool tie = exiled == null && seededTie;
                if (exiled == null)
                {
                    exiled = VotingUtils.GetExiled(processEvent.Votes, out tie);
                }

                hud.RpcVotingComplete(voterStates, exiled, tie);
            }

            // Random Impostor Role Keybind (F3)
            if (Input.GetKeyDown(KeyCode.F3) && TownOfUsPlugin.IsDevBuild && !TownOfUsPlugin.IsBetaBuild && LobbyBehaviour.Instance)
            {
                var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                var roleOptions = currentGameOptions.RoleOptions;
                var impostorRoles = MiscUtils.SpawnableRoles
                    .Where(role => role.IsImpostor() && roleOptions.GetNumPerGame(role.Role) > 0)
                    .ToList();

                if (impostorRoles.Count > 0)
                {
                    var randomRole = impostorRoles[Random.Range(0, impostorRoles.Count)];
                    var roleIdentifier = randomRole is ITownOfUsRole touRole ? touRole.LocaleKey : randomRole.GetRoleName();
                    var playerName = PlayerControl.LocalPlayer.Data.PlayerName;
                    UpCommandRequests.SetRequest(playerName, roleIdentifier);
                }
            }

            // Random Neutral Killer Role Keybind (F4)
            if (Input.GetKeyDown(KeyCode.F4) && TownOfUsPlugin.IsDevBuild && !TownOfUsPlugin.IsBetaBuild && LobbyBehaviour.Instance)
            {
                var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                var roleOptions = currentGameOptions.RoleOptions;
                var neutralKillerRoles = MiscUtils.SpawnableRoles
                    .Where(role => role.GetRoleAlignment() == RoleAlignment.NeutralKilling && roleOptions.GetNumPerGame(role.Role) > 0)
                    .ToList();

                if (neutralKillerRoles.Count > 0)
                {
                    var randomRole = neutralKillerRoles[Random.Range(0, neutralKillerRoles.Count)];
                    var roleIdentifier = randomRole is ITownOfUsRole touRole ? touRole.LocaleKey : randomRole.GetRoleName();
                    var playerName = PlayerControl.LocalPlayer.Data.PlayerName;
                    UpCommandRequests.SetRequest(playerName, roleIdentifier);
                }
            }
        }

        // CTRL to pass through objects in lobby
        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined &&
            (TutorialManager.InstanceExists || OptionGroupSingleton<HostSpecificOptions>.Instance.LobbyFunMode.Value))
        {
            var player = PlayerControl.LocalPlayer;
            if (player != null && player.gameObject != null)
            {
                var ctrlHeld = Input.GetKey(KeyCode.LeftControl);
                var ghostLayer = LayerMask.NameToLayer("Ghost");

                if (ctrlHeld && !_wasCtrlHeld)
                {
                    _originalPlayerLayer = player.gameObject.layer;
                    player.gameObject.layer = ghostLayer;
                }
                else if (!ctrlHeld && _wasCtrlHeld && _originalPlayerLayer.HasValue)
                {
                    player.gameObject.layer = _originalPlayerLayer.Value;
                    _originalPlayerLayer = null;
                }

                _wasCtrlHeld = ctrlHeld;
            }
        }
        else
        {
            // Reset layer when game starts (GameState != Joined) or if keybinds are disabled
            var player = PlayerControl.LocalPlayer;
            if (player != null && player.gameObject != null && _originalPlayerLayer.HasValue)
            {
                player.gameObject.layer = _originalPlayerLayer.Value;
                _originalPlayerLayer = null;
            }

            _wasCtrlHeld = false;
        }

        if (!PlayerControl.LocalPlayer.Data.IsDead && !PlayerControl.LocalPlayer.IsImpostor())
        {
            var kill = __instance.KillButton;
            var vent = __instance.ImpostorVentButton;

            if (kill.isActiveAndEnabled)
            {
                var killKey = ReInput.players.GetPlayer(0).GetButtonDown("ActionSecondary");
                var controllerKill = ConsoleJoystick.player.GetButtonDown(8);
                if (killKey || controllerKill)
                {
                    kill.DoClick();
                }
            }

            if (vent.isActiveAndEnabled)
            {
                var ventKey = ReInput.players.GetPlayer(0).GetButtonDown("UseVent");
                var controllerVent = ConsoleJoystick.player.GetButtonDown(50);
                if (ventKey || controllerVent)
                {
                    vent.DoClick();
                }
            }
        }

        if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick)
        {
            return;
        }

        var contPlayer = ConsoleJoystick.player;
        var buttonList = CustomButtonManager.Buttons.Where(x =>
            x.Enabled(PlayerControl.LocalPlayer.Data.Role) && x.Button != null && x.Button.isActiveAndEnabled &&
            x.CanUse()).ToList();

        foreach (var button in buttonList.Where(x => x is TownOfUsButton))
        {
            var touButton = button as TownOfUsButton;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }

        foreach (var button in buttonList.Where(x => x is TownOfUsTargetButton<DeadBody>))
        {
            var touButton = button as TownOfUsTargetButton<DeadBody>;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }

        foreach (var button in buttonList.Where(x => x is TownOfUsTargetButton<Vent>))
        {
            var touButton = button as TownOfUsTargetButton<Vent>;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }

        foreach (var button in buttonList.Where(x => x is TownOfUsTargetButton<PlayerControl>))
        {
            var touButton = button as TownOfUsTargetButton<PlayerControl>;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }
    }
}