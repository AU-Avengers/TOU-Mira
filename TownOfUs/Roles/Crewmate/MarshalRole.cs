using System.Globalization;
using System.Text;
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
using TownOfUs.Events.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
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

    public int TribunalsLeft { get; set; }
    public static bool TribunalHappening { get; set; }
    public static readonly Dictionary<byte, TextMeshPro> VoteNumbers = new();

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
        
        TouAudio.PlaySound(TouAudio.TribunalSound);
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Marshal, 1.5f, 0.1f));
        var notif1 = Helpers.CreateAndShowNotification("<b>A tribunal has started!</b>", Color.white, spr: TouRoleIcons.Marshal.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);
        notif1.transform.localPosition = new Vector3(0f, 1f, -20f);

        TribunalHappening = true;
        marshalRole.TribunalsLeft--;
        player.GetVoteData().IncreaseRemainingVotes((int)OptionGroupSingleton<MarshalOptions>.Instance.MarshalExtraVotes);
        CreateVoteRequirementText();
        
        VoteNumbers.Clear();
        foreach (var pva in MeetingHud.Instance.playerStates)
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
            voteCountIcon.sprite = MeetingHud.Instance.PlayerVotePrefab.sprite;
            var voteCount = voteCountIcon.transform.FindChild("LevelNumber").GetComponent<TextMeshPro>();
            voteCount.gameObject.name = "VotesNumber";
            voteCount.transform.localPosition = new Vector3(-0.02f, 0f);
            voteCount.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            voteCount.text = "<b>0</b>";
            VoteNumbers.Add(voteAreaPlayer.PlayerId, voteCount);
            
            pva.ThumbsDown.enabled = false;
        }
        
        MeetingHud.Instance.ClearVote();
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
    }

    private static void CreateVoteRequirementText()
    {
        var text = GameObject.Instantiate(MeetingHud.Instance.TimerText, MeetingHud.Instance.ButtonParent.transform);
        text.transform.position -= new Vector3(3, 0);
        text.gameObject.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        text.text = "4 votes needed to eject";
    }
}