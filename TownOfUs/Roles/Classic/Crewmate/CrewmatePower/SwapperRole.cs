using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SwapperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, ILoyalCrewmate
{
    public bool CanBeTraitor => false;
    public bool CanBeCrewpostor => false;
    public bool CanBeEgotist => true;
    public bool CanBeOtherEvil => true;

    private MeetingMenu meetingMenu;

    [HideFromIl2Cpp] public PlayerVoteArea Swap1 { get; set; }
    [HideFromIl2Cpp] public PlayerVoteArea Swap2 { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Swapper";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(TouLocale.GetParsed($"TouRole{LocaleKey}SwapWiki", "Swap (Meeting)"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}SwapWikiDescription"),
                    TouAssets.SwapActive)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Swapper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public bool IsPowerCrew => true; // Always disable end game checks because a swapper can still screw people over

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Swapper.LoadAsset(), "TouMira.Role.Crewmate.Swapper", 1.45f),
        Icon = TouRoleIcons.Swapper,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 1,
        IntroSound = TouAudio.TimeLordIntroSound
    };



    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(this, SetActive, MeetingAbilityType.Toggle, LegacyAssets.IsLegacy ? LegacyAssets.SwapActive : TouAssets.SwapActive,
                LegacyAssets.IsLegacy ? LegacyAssets.SwapInactive : TouAssets.SwapInactive, IsExempt)
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

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
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
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;

        return !player || !player?.Data || player!.Data.Disconnected || player.Data.IsDead ||
               player.HasModifier<JailedModifier>();
    }

    private void SetActive(PlayerVoteArea voteArea, MeetingHud __instance)
    {
        if (__instance.state == MeetingHud.MeetingStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (!Swap1)
        {
            Swap1 = voteArea;
            meetingMenu.Actives[voteArea.PlayerId] = true;
        }
        else if (!Swap2)
        {
            Swap2 = voteArea;
            meetingMenu.Actives[voteArea.PlayerId] = true;
        }
        else if (Swap1 == voteArea)
        {
            meetingMenu.Actives[Swap1!.PlayerId] = false;
            Swap1 = null!;
        }
        else if (Swap2 == voteArea)
        {
            meetingMenu.Actives[Swap2!.PlayerId] = false;
            Swap2 = null!;
        }
        else
        {
            meetingMenu.Actives[Swap1!.PlayerId] = false;
            Swap1 = Swap2;
            Swap2 = voteArea;
            meetingMenu.Actives[voteArea.PlayerId] = !meetingMenu.Actives[voteArea.PlayerId];
        }

        RpcSyncSwaps(Player, Swap1?.PlayerId ?? 255, Swap2?.PlayerId ?? 255);
    }

    [MethodRpc((uint)TownOfUsRpc.SetSwaps)]
    public static void RpcSyncSwaps(PlayerControl swapper, byte swap1, byte swap2)
    {
        var swapperRole = swapper.Data?.Role as SwapperRole;
        var areas = MeetingHud.Instance.playerStates.ToList();
        swapperRole!.Swap1 = areas.Find(x => x.PlayerId == swap1)!;
        swapperRole.Swap2 = areas.Find(x => x.PlayerId == swap2)!;
    }
}