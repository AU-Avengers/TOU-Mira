using System.Collections;
using System.Globalization;
using System.Text;
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
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MarshalRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
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
        IntroSound = TouAudio.TribunalSound
    };

    public bool IsPowerCrew => true;

    public int TribunalsLeft { get; private set; }
    
    private static TextMeshPro votesRequiredText { get; set; }
    public static bool TribunalHappening { get; set; }
    public static int RequiredVotes { get; private set; }
    public static List<NetworkedPlayerInfo> EjectedPlayers { get; } = new();
    public static Dictionary<byte, TextMeshPro> VoteNumbers { get; } = new();

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

        votesRequiredText = null;
        TribunalHappening = false;
        RequiredVotes = 0;
        EjectedPlayers.Clear();
        VoteNumbers.Clear();
        
        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                Click,
                MeetingAbilityType.Click,
                TouAssets.TribunalSprite,
                TouAssets.RetrainSprite,
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

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        if (!Player.AmOwner)
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

        var meetingHud = MeetingHud.Instance;
        
        TouAudio.PlaySound(TouAudio.TribunalSound);
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Marshal, 1.5f, 0.1f));
        
        var notif1 = Helpers.CreateAndShowNotification("<b>A tribunal has started!</b>", Color.white, spr: TouRoleIcons.Marshal.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);
        notif1.transform.localPosition = new Vector3(0f, 1f, -20f);

        TribunalHappening = true;
        marshalRole.TribunalsLeft--;
        player.GetVoteData().IncreaseRemainingVotes((int)OptionGroupSingleton<MarshalOptions>.Instance.MarshalExtraVotes);
        
        UpdateVoteRequirement();
        
        VoteNumbers.Clear();
        EjectedPlayers.Clear();
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
            var votes = voteData.Votes.RemoveAll(x => true);
            voteData.VotesRemaining += votes;
            
            var voteCountIcon = GameObject.Instantiate(pva.LevelNumberText.transform.parent.gameObject, pva.transform).GetComponent<SpriteRenderer>();
            voteCountIcon.gameObject.name = "VoteCount";
            voteCountIcon.transform.localPosition = new Vector3(-0.5f, 0.16f, -50);
            voteCountIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1);
            voteCountIcon.transform.FindChild("LevelLabel").gameObject.Destroy();
            voteCountIcon.sprite = meetingHud.PlayerVotePrefab.sprite;
            voteCountIcon.material = meetingHud.PlayerVotePrefab.material;
            PlayerMaterial.SetColors(Palette.DisabledGrey, voteCountIcon);
            var voteCount = voteCountIcon.transform.FindChild("LevelNumber").GetComponent<TextMeshPro>();
            voteCount.gameObject.name = "VotesNumber";
            voteCount.transform.localPosition = new Vector3(-0.02f, 0f);
            voteCount.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            voteCount.text = "<b>0</b>";
            VoteNumbers.Add(voteAreaPlayer.PlayerId, voteCount);
            
            pva.ThumbsDown.enabled = false;
        }
        
        meetingHud.ClearVote();
        meetingHud.SkipVoteButton.gameObject.SetActive(false);
    
        // Roles with abilities that can change the meeting result are disabled during a tribunal
        switch (PlayerControl.LocalPlayer.Data.Role)
        {
            case SwapperRole:
            case ProsecutorRole:
                MeetingMenu.Instances.Do(x => x.HideButtons());
                
                if (PlayerControl.LocalPlayer.Data.Role is SwapperRole swapperRole)
                {
                    swapperRole.Swap1 = null;
                    swapperRole.Swap2 = null;
                }
                break;
        }

        AdjustTimeRemaining();
    }

    [MethodRpc((uint)TownOfUsRpc.TribunalEjection)]
    private static void RpcTribunalEjection(PlayerControl victim)
    {
        if (EjectedPlayers.Contains(victim.Data)) return;
        
        EjectedPlayers.Add(victim.Data);
        ResetAllVotes();
        
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(victim.KillSfx, false, 0.8f);
        }
        
        MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));
        if (victim.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideButtons());
        }

        var victimPva = MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == victim.PlayerId);
        if (victimPva != null)
        {
            victimPva.SetDisabled();
            victimPva.XMark.gameObject.SetActive(true);
        }
        
        AdjustTimeRemaining();
        UpdateVoteRequirement();
        UpdateVoteNumbers();
    }
    
    [MethodRpc((uint)TownOfUsRpc.EndTribunal)]
    public static void RpcEndTribunal(PlayerControl player)
    {
        Logger<TownOfUsPlugin>.Warning($"RpcEndTribunal");
        var instance = MeetingHud.Instance;
        AmongUsClient.Instance.DisconnectHandlers.Remove(MeetingHud.Instance.Cast<IDisconnectHandler>());
        PlayerControl.AllPlayerControls.ToArray().Do(p => p.Data.Role.OnVotingComplete());

        instance.state = MeetingHud.VoteStates.Results;
        PlayerControl.LocalPlayer.GetVoteData().VotesRemaining = 0;
        instance.TimerText.gameObject.SetActive(false);

        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
            ControllerManager.Instance.CloseOverlayMenu(DestroyableSingleton<HudManager>.Instance.Chat.name);
        }

        ControllerManager.Instance.CloseOverlayMenu(instance.name);
        if (EjectedPlayers.Count > 0)
        {
            Coroutines.Start(CoEjectionCutscene(EjectedPlayers[0], false, true));
            EjectedPlayers.RemoveAt(0);
        }
        else
        {
            Coroutines.Start(CoEjectionCutscene(null, true, true));
        }
    }
    
    public static IEnumerator CoEjectionCutscene(NetworkedPlayerInfo? exiled, bool skipped = false, bool first = false)
    {
        Logger<TownOfUsPlugin>.Warning($"CoEjectionCutscene");
        
        if (first)
        {
            Logger<TownOfUsPlugin>.Warning($"The cutscene is played for the first time");
            
            yield return new WaitForSeconds(4);
            HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 1f));
            
            ConsoleJoystick.SetMode_Task();
            MeetingHud.Instance.DespawnOnDestroy = false;
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(PlayerControl.LocalPlayer.Data.IsDead);
            DestroyableSingleton<HudManager>.Instance.Chat.HideBanButton();
            
            MeetingHud.Instance.gameObject.DestroyImmediate();
        }
        
        ExileController exileController;
        if (skipped)
        {
            Logger<TownOfUsPlugin>.Warning($"The tribunal is skipped");
            exileController = GameObject.Instantiate(ShipStatus.Instance.ExileCutscenePrefab, DestroyableSingleton<HudManager>.Instance.transform);
            exileController.transform.localPosition = new Vector3(0f, 0f, -60f);
            exileController.BeginForGameplay(null, false);
            yield break;
        }
        
        Logger<TownOfUsPlugin>.Warning($"Creating exile controller for {exiled.PlayerName}");
        exileController = GameObject.Instantiate(ShipStatus.Instance.ExileCutscenePrefab, DestroyableSingleton<HudManager>.Instance.transform);
        exileController.transform.localPosition = new Vector3(0f, 0f, -60f);
        exileController.BeginForGameplay(exiled, false);
    }

    public static void CheckForEjection()
    {
        var mostVotes = VotingUtils.CalculateNumVotes(VotingUtils.CalculateVotes()
                    .Where(v => !EjectedPlayers.FirstOrDefault(p => p.PlayerId == v.Voter)))
                    .MaxPair(out _);
        
        if (mostVotes.Value >= RequiredVotes)
        {
            var player = MiscUtils.PlayerById(mostVotes.Key);
            if (player == null) return;
            
            Logger<TownOfUsPlugin>.Warning($"Player {player.Data.PlayerName} has enough votes to be ejected");
            RpcTribunalEjection(player);
        }

        if (EjectedPlayers.Count >= OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunalEjections)
        {
            Logger<TownOfUsPlugin>.Warning($"{EjectedPlayers.Count} ejected players - Ending the tribunal");
            RpcEndTribunal(PlayerControl.LocalPlayer);
        }
    }

    private static void UpdateVoteRequirement()
    {
        // Grab all alive players, minus the players marked as ejected
        var validPlayers = Helpers.GetAlivePlayers()
            .Where(p => !EjectedPlayers.Contains(p.Data))
            .ToList();
        
        RequiredVotes = (validPlayers.Count / 2) + 1;
        
        if (!votesRequiredText)
        {
            votesRequiredText = GameObject.Instantiate(MeetingHud.Instance.TimerText, MeetingHud.Instance.ButtonParent.transform);
            votesRequiredText.transform.position -= new Vector3(3, 0);
            votesRequiredText.gameObject.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        }
        
        votesRequiredText.text = $"{RequiredVotes} votes needed to eject";
    }
    
    public static void UpdateVoteNumbers()
    {
        foreach (var pva in MeetingHud.Instance.playerStates)
        {
            if (VoteNumbers.TryGetValue(pva.TargetPlayerId, out var num))
                num.text = GetVotesRecievied(pva).Count.ToString(TownOfUsPlugin.Culture);
        }
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
        var renderers = pva.transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (var rend in renderers)
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
        meetingHud.discussionTimer = (logicOptions.GetDiscussionTime() + logicOptions.GetVotingTime()) - OptionGroupSingleton<MarshalOptions>.Instance.TribunalEjectionTime;
    }
    
    private static List<CustomVote> GetVotesRecievied(PlayerVoteArea player)
    {
        return
        [
            .. Helpers.GetAlivePlayers()
                .SelectMany(plr => plr.GetVoteData().Votes).Where(data => data.Suspect == player.TargetPlayerId)
        ];
    }
}