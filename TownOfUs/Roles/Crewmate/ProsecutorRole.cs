using System.Globalization;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Roles.Crewmate;

public sealed class ProsecutorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp]
    public PlayerVoteArea? ProsecuteButton { get; private set; }

    public bool HasProsecuted { get; private set; }

    public byte ProsecuteVictim { get; set; } = byte.MaxValue;

    public bool SelectingProsecuteVictim { get; set; }

    public int ProsecutionsCompleted { get; set; }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not ProsecutorRole)
        {
            return;
        }

        var meeting = MeetingHud.Instance;

        if (!Player.AmOwner || meeting == null || ProsecuteButton == null)
        {
            return;
        }

        ProsecuteButton.gameObject.SetActive(meeting.SkipVoteButton.gameObject.active && !SelectingProsecuteVictim);

        if (!ProsecuteButton.gameObject.active)
        {
            return;
        }

        if (meeting.state == MeetingHud.VoteStates.Discussion &&
            meeting.discussionTimer < GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime)
        {
            ProsecuteButton.SetDisabled();
        }
        else
        {
            ProsecuteButton.SetEnabled();
        }

        ProsecuteButton.voteComplete = meeting.SkipVoteButton.voteComplete;
    }

    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Prosecutor, "Prosecutor");
    public string RoleDescription => "Exile Players Of Your Choosing";
    public string RoleLongDescription => "Choose to exile anyone you want";
    public Color RoleColor => TownOfUsColors.Prosecutor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public bool IsPowerCrew =>
        ProsecutionsCompleted <
        (int)OptionGroupSingleton<ProsecutorOptions>.Instance
            .MaxProsecutions; // Disable end game checks if prosecutes are available

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Prosecutor,
        IntroSound = TouAudio.ProsIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = ITownOfUsRole.SetNewTabText(this);
        if (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            text.AppendLine(CultureInfo.InvariantCulture, $"<b>You may prosecute crew.</b>");
        }

        var prosecutes = OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions - ProsecutionsCompleted;
        var newText = prosecutes == 1 ? "1 Prosecution Remaining." : $"\n{prosecutes} Prosecutions Remaining.";
        text.AppendLine(CultureInfo.InvariantCulture, $"{newText}");
        return text;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Power role that can Exile a player, applying 5 votes to a player of their choosing. They can also see who voted for who, even if they’re anonymous."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Prosecute (Meeting)",
            "Exile any player of your choosing, throwing 5 votes on them and ignoring all other votes.",
            TouRoleIcons.Prosecutor)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.HasModifier<ImitatorCacheModifier>())
        {
            ProsecutionsCompleted = (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var meeting = MeetingHud.Instance;
        if (!Player.AmOwner || meeting == null ||
            ProsecutionsCompleted >= OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
        {
            return;
        }

        var skip = meeting.SkipVoteButton;
        ProsecuteButton = Instantiate(skip, skip.transform.parent);
        ProsecuteButton.Parent = meeting;
        ProsecuteButton.SetTargetPlayerId(251);
        ProsecuteButton.transform.localPosition = skip.transform.localPosition + new Vector3(0f, -0.17f, 0f);

        ProsecuteButton.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
        ProsecuteButton.gameObject.GetComponentInChildren<TextMeshPro>().text = "PROSECUTE";

        foreach (var plr in meeting.playerStates.AddItem(skip))
        {
            plr.gameObject.GetComponentInChildren<PassiveButton>().OnClick
                .AddListener((UnityAction)(() => ProsecuteButton.ClearButtons()));
        }

        skip.transform.localPosition += new Vector3(0f, 0.20f, 0f);
    }

    public void Cleanup()
    {
        ProsecuteButton = null;
        SelectingProsecuteVictim = false;
        ProsecuteVictim = byte.MaxValue;

        if (HasProsecuted)
        {
            ProsecutionsCompleted++;
        }

        HasProsecuted = false;
    }

    [MethodRpc((uint)TownOfUsRpc.Prosecute)]
    public static void RpcProsecute(PlayerControl plr, byte Victim)
    {
        if (plr.Data.Role is not ProsecutorRole prosecutorRole)
        {
            return;
        }

        if (prosecutorRole.ProsecutionsCompleted >=
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
        {
            return;
        }

        prosecutorRole.HasProsecuted = true;
        prosecutorRole.ProsecuteVictim = Victim;
    }
}