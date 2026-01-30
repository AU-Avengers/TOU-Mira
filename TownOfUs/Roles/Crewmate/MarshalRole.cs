using System.Collections;
using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Crewmate;

public sealed class MarshalRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, ILoyalCrewmate
{
    public bool CanBeTraitor => false;
    public bool CanBeCrewpostor => false;
    public bool CanBeEgotist => true;
    public bool CanBeOtherEvil => true;
    private MeetingMenu meetingMenu;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => "Marshal";
    public string RoleDescription => "Call a military tribunal!";
    public string RoleLongDescription => $"Call a military tribunal, allowing the crew to\neject {OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunalEjections} players in one meeting";
    public Color RoleColor => TownOfUsColors.Marshal;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Marshal,
        MaxRoleCount = 1,
        IntroSound = TouAudio.TribunalSound
    };

    public bool IsPowerCrew => TribunalsLeft > 0;

    public int TribunalsLeft { get; private set; }
    
    public static bool TribunalHappening { get; set; }
    public static int RequiredVotes { get; private set; }
    public static List<PlayerControl> EjectedPlayers { get; } = [];

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>{TribunalsLeft}/{OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunals} Tribunals Left</b>");

        return stringB;
    }
    
    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Power role that can call a tribunal during a meeting. The tribunal allows the crew to eject {OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunalEjections} players in one meeting."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Tribunal (Meeting)",
            $"Call a military tribunal, allowing the crew to eject {OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunalEjections} players in one meeting.",
            TouAssets.TribunalClearSprite)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        TribunalsLeft = (int)OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunals;
        Cleanup();
        
        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                Click,
                "Tribunal",
                MeetingAbilityType.Click,
                TouAssets.TribunalClearSprite,
                TouAssets.TribunalClearSprite,
                IsExempt,
                hoverColor: RoleColor)
            {
                Position = new Vector3(-0.40f, 0f, -3f),
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        
        if (TribunalsLeft <= 0) return;
        if (DeathEventHandlers.CurrentRound < (int)OptionGroupSingleton<MarshalOptions>.Instance.RoundWhenAvailable) return;

        if (Player.AmOwner)
        {
            meetingMenu.GenButtons(MeetingHud.Instance,
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud meetingHud)
    {
        if (meetingHud.state == MeetingHud.VoteStates.Discussion)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        if (TribunalsLeft <= 0)
        {
            return;
        }

        meetingMenu.HideButtons();
        RpcTribunal(Player);
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId != Player.PlayerId;
    }

    private static void Cleanup()
    {
        TribunalHappening = false;
        RequiredVotes = 0;
        EjectedPlayers.Clear();
    }

    [MethodRpc((uint)TownOfUsRpc.Tribunal)]
    private static void RpcTribunal(PlayerControl player)
    {
        if (player.Data.Role is not MarshalRole marshalRole)
        {
            return;
        }
        if (!MeetingHud.Instance)
        {
            return;
        }
        if (!player.HasModifier<MayorRevealModifier>())
        {
            player.AddModifier<MayorRevealModifier>(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MarshalRole>()));
        }
        if (player.TryGetModifier<ToBecomeTraitorModifier>(out var traitorMod))
        {
            traitorMod.Clear();
        }
        var meetingHud = MeetingHud.Instance;
        
        TouAudio.PlaySound(TouAudio.TribunalSound);
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Marshal, 1.5f, 0.1f));
        
        var tribunalNotif = Helpers.CreateAndShowNotification("<b>A tribunal has started!</b>", Color.white, spr: TouRoleIcons.Marshal.LoadAsset());
        tribunalNotif.Text.SetOutlineThickness(0.35f);
        tribunalNotif.transform.localPosition = new Vector3(0f, 1f, -20f);

        TribunalHappening = true;
        marshalRole.TribunalsLeft--;
        EjectedPlayers.Clear();
        player.GetVoteData().IncreaseRemainingVotes((int)OptionGroupSingleton<MarshalOptions>.Instance.MarshalExtraVotes);
        
        // Votes required text
        var text = MeetingHud.Instance.SkippedVoting.GetComponentInChildren<TextMeshPro>();
        text.gameObject.GetComponent<TextTranslatorTMP>()?.DestroyImmediate();
        text.transform.parent.gameObject.SetActive(true);
        UpdateVoteRequirement();
        
        // Remove the votes of all players
        foreach (var pva in meetingHud.playerStates)
        {
            pva.UnsetVote();

            var voteAreaPlayer = MiscUtils.PlayerById(pva.TargetPlayerId);
            if (voteAreaPlayer == null)
            {
                continue;
            }
            if (voteAreaPlayer.HasDied())
            {
                continue;
            }

            var voteData = voteAreaPlayer.GetVoteData();
            voteData.VotesRemaining += voteData.Votes.Count;
            voteData.Votes.Clear();
            pva.ThumbsDown.enabled = false;
        }
        
        meetingHud.ClearVote();
        meetingHud.SkipVoteButton.gameObject.SetActive(false);

        foreach (var pros in CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>())
        {
            if (pros.HasProsecuted && pros.ProsecuteVictim != byte.MaxValue)
            {
                pros.HasProsecuted = false;
                pros.ProsecutionsCompleted++;
                pros.ProsecuteVictim = byte.MaxValue;
            }
        }

        foreach (var swapper in CustomRoleUtils.GetActiveRolesOfType<SwapperRole>())
        {
            swapper.Swap1 = null;
            swapper.Swap2 = null;
        }
        MeetingMenu.Instances.Do(x => x.HideButtons());

        AdjustTimeRemaining();
    }

    [MethodRpc((uint)TownOfUsRpc.TribunalEjection)]
    private static void RpcTribunalEjection(PlayerControl victim)
    {
        if (EjectedPlayers.Contains(victim))
        {
            return;
        }
        
        EjectedPlayers.Add(victim);
        ResetAllVotes();
        
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(victim.KillSfx, false, 0.8f);
        }

        MeetingMenu.Instances.Do(x => x.HideButtons());

        var victimPva = MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == victim.PlayerId);
        if (victimPva != null)
        {
            victimPva.SetDisabled();
            victimPva.XMark.gameObject.SetActive(true);
        }
        
        MeetingHud.Instance.TimerText.color = Color.white;
        
        AdjustTimeRemaining();
        UpdateVoteRequirement();
    }
    
    [MethodRpc((uint)TownOfUsRpc.EndTribunal)]
    public static void RpcEndTribunal(PlayerControl player)
    {
        Warning($"RpcEndTribunal");
        var instance = MeetingHud.Instance;
        AmongUsClient.Instance.DisconnectHandlers.Remove(MeetingHud.Instance.Cast<IDisconnectHandler>());
        PlayerControl.AllPlayerControls.ToArray().Do(p => p.Data.Role.OnVotingComplete());

        instance.state = MeetingHud.VoteStates.Results;
        PlayerControl.LocalPlayer.GetVoteData().VotesRemaining = 0;
        instance.TimerText.gameObject.SetActive(false);
        instance.ProceedButton.gameObject.SetActive(false);
        instance.playerStates.Do(x => x.SetDisabled());

        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            HudManager.Instance.Chat.ForceClosed();
            ControllerManager.Instance.CloseOverlayMenu(HudManager.Instance.Chat.name);
        }

        ControllerManager.Instance.CloseOverlayMenu(instance.name);
        if (EjectedPlayers.Count > 0)
        {
            Coroutines.Start(CoEjectionCutscene(EjectedPlayers[0].Data, false, true));
            EjectedPlayers.RemoveAt(0);
        }
        else
        {
            Coroutines.Start(CoEjectionCutscene(null, true, true));
        }
    }
    
    public static IEnumerator CoEjectionCutscene(NetworkedPlayerInfo? exiled, bool skipped = false, bool first = false)
    {
        Warning($"CoEjectionCutscene");

        if (first)
        {
            Warning($"The cutscene is played for the first time");

            yield return new WaitForSeconds(3);

            ConsoleJoystick.SetMode_Task();
            MeetingHud.Instance.DespawnOnDestroy = false;
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }

            HudManager.Instance.Chat.SetVisible(PlayerControl.LocalPlayer.Data.IsDead);
            HudManager.Instance.Chat.HideBanButton();

            HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 1f));

            yield return new WaitForSeconds(1);

            try
            {
                MeetingHud.Instance.gameObject.DestroyImmediate(); // breaks sometimes so this mf is behind bars for now
            }
            catch
            {
                // ignored
            }
        }

        ExileController exileController = Object.Instantiate(ShipStatus.Instance.ExileCutscenePrefab, HudManager.Instance.transform);
        exileController.transform.localPosition = new Vector3(0f, 0f, -60f);
        if (skipped)
        {
            Warning($"The tribunal is skipped");
            exileController.BeginForGameplay(null, false);
            yield break;
        }
        
        Warning($"Creating exile controller for {exiled!.PlayerName}");
        exileController.BeginForGameplay(exiled, false);
    }

    public static void CheckForEjection()
    {
        var mostVotes = VotingUtils
            .CalculateNumVotes(VotingUtils.CalculateVotes()
            .Where(v => !EjectedPlayers.FirstOrDefault(p => p.PlayerId == v.Voter)))
            .MaxPair(out _);
        
        if (mostVotes.Value >= RequiredVotes)
        {
            var player = MiscUtils.PlayerById(mostVotes.Key);
            if (player == null)
            {
                return;
            }
            
            Warning($"Player {player.Data.PlayerName} has enough votes to be ejected");
            RpcTribunalEjection(player);
        }

        if (EjectedPlayers.Count >= OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunalEjections)
        {
            Warning($"{EjectedPlayers.Count} ejected players - Ending the tribunal");
            RpcEndTribunal(PlayerControl.LocalPlayer);
        }
    }

    private static void UpdateVoteRequirement()
    {
        var validPlayers = Helpers.GetAlivePlayers()
            .Where(p => !EjectedPlayers.Contains(p))
            .ToList();
        
        RequiredVotes = validPlayers.Count / 2 + 1;
        var text = MeetingHud.Instance.SkippedVoting.GetComponentInChildren<TextMeshPro>();
        text.text = $"<size=80%>{RequiredVotes} votes needed to eject</size>";
    }

    private static void ResetAllVotes()
    {
        PlayerControl.AllPlayerControls.ToArray().Do(p =>
        {
            p.GetVoteData().Votes.Clear();
            MeetingHud.Instance.playerStates.Do(pva =>
            {
                RemoveBloopsOfId(p.PlayerId, pva);
                pva.ThumbsDown.enabled = false;
            });
        });
    }

    public static void RemoveBloopsOfId(byte id, PlayerVoteArea pva)
    {
        foreach (var rend in pva.transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (rend.name == id.ToString(TownOfUsPlugin.Culture))
            {
                pva.GetComponent<VoteSpreader>().Votes.Remove(rend);
                rend.gameObject.DestroyImmediate();
            }
        }
    }

    private static void AdjustTimeRemaining()
    {
        var meetingHud = MeetingHud.Instance;
        var logicOptions = GameManager.Instance.LogicOptions.TryCast<LogicOptionsNormal>();
        meetingHud.discussionTimer = (logicOptions!.GetDiscussionTime() + logicOptions.GetVotingTime()) - OptionGroupSingleton<MarshalOptions>.Instance.TribunalEjectionTime;
    }
}