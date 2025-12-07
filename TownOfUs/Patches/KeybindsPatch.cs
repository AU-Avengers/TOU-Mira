using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Rewired;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

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

        var isHost = PlayerControl.LocalPlayer.IsHost();

        //  Full List of binds:
        //      Suicide Keybind (ENTER + T + Left Shift)
        //      End Game Keybind (ENTER + L + Left Shift)
        //      Start Meeting (ENTER + K + Left Shift)
        //      End Meeting Keybind (F6)
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
                        gameFlow.Manager.RpcEndGame(GameOverReason.ImpostorsByKill, false);
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
                var voterStates = new Il2CppStructArray<MeetingHud.VoterState>(areas.Length);

                var votes = new List<CustomVote>();
                for (int i = 0; i < areas.Length; i++)
                {
                    var area = areas[i];
                    byte voterId = area.TargetPlayerId;
                    byte votedFor = area.VotedFor;

                    voterStates[i] = new MeetingHud.VoterState
                    {
                        VoterId = voterId,
                        VotedForId = votedFor
                    };

                    if (votedFor != byte.MaxValue && votedFor != voterId)
                    {
                        votes.Add(new CustomVote(voterId, votedFor));
                    }
                }

                var processEvent = new ProcessVotesEvent(votes);
                MiraEventManager.InvokeEvent(processEvent);

                var exiled = processEvent.ExiledPlayer;
                bool tie = false;

                if (exiled == null)
                {
                    exiled = VotingUtils.GetExiled(processEvent.Votes, out tie);
                }

                hud.RpcVotingComplete(voterStates, exiled, tie);
            }
        }

        // CTRL to pass through objects in lobby
        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined)
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
