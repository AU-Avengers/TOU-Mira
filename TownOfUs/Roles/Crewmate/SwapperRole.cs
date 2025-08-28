using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SwapperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu meetingMenu;

    [HideFromIl2Cpp]
    public PlayerVoteArea? Swap1 { get; set; }
    [HideFromIl2Cpp]
    public PlayerVoteArea? Swap2 { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => TouLocale.Get(TouNames.Swapper, "Swapper");
    public string RoleDescription => "Swap Votes To Save The Crew!";
    public string RoleLongDescription => "Swap votes from one player to another during a meeting";
    public Color RoleColor => TownOfUsColors.Swapper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public bool IsPowerCrew => true; // Always disable end game checks because a swapper can still screw people over

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Swapper,
        MaxRoleCount = 1,
        IntroSound = TouAudio.TimeLordIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Power that can swap the votes of two players in a meeting. " +
            "Their meeting vote areas will be swapped visually, and the votes will be swapped. " +
            "If player 1 recieved the most votes, and you swap them with player 2, player 2 will now be ejected instead. "
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Swap (Meeting)",
            "Select two players to swap votes for, and at the end of the meeting, they will swap spots!",
            TouAssets.SwapActive)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(this, SetActive, MeetingAbilityType.Toggle, TouAssets.SwapActive,
                TouAssets.SwapInactive, IsExempt)
            {
                Position = new Vector3(-0.40f, 0f, -3f)
            };
        }

        if (!OptionGroupSingleton<SwapperOptions>.Instance.CanButton)
        {
            player.RemainingEmergencies = 0;
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

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

    private static bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;

        return !player || !player?.Data || player!.Data.Disconnected || player.Data.IsDead ||
               player.HasModifier<JailedModifier>();
    }

    private void SetActive(PlayerVoteArea voteArea, MeetingHud __instance)
    {
        if (__instance.state == MeetingHud.VoteStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (!Swap1)
        {
            Swap1 = voteArea;
            meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        }
        else if (!Swap2)
        {
            Swap2 = voteArea;
            meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        }
        else if (Swap1 == voteArea)
        {
            meetingMenu.Actives[Swap1!.TargetPlayerId] = false;
            Swap1 = null;
        }
        else if (Swap2 == voteArea)
        {
            meetingMenu.Actives[Swap2!.TargetPlayerId] = false;
            Swap2 = null;
        }
        else
        {
            meetingMenu.Actives[Swap1!.TargetPlayerId] = false;
            Swap1 = Swap2;
            Swap2 = voteArea;
            meetingMenu.Actives[voteArea.TargetPlayerId] = !meetingMenu.Actives[voteArea.TargetPlayerId];
        }

        RpcSyncSwaps(Player, Swap1?.TargetPlayerId ?? 255, Swap2?.TargetPlayerId ?? 255);
    }

    [MethodRpc((uint)TownOfUsRpc.SetSwaps)]
    public static void RpcSyncSwaps(PlayerControl swapper, byte swap1, byte swap2)
    {
        var swapperRole = swapper.Data?.Role as SwapperRole;
        var areas = MeetingHud.Instance.playerStates.ToList();
        swapperRole!.Swap1 = areas.Find(x => x.TargetPlayerId == swap1);
        swapperRole.Swap2 = areas.Find(x => x.TargetPlayerId == swap2);
    }
}