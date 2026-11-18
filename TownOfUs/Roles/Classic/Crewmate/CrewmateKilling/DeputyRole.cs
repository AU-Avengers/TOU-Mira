using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.AnimationSystem.SpecialKills;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class DeputyRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, IMeetingKiller
{
    public bool ShotThisMeeting;
    public void TriggerMeetingAnimation(PlayerControl source, PlayerControl target, PlayerVoteArea targetVoteArea,
        int associatedAnimKey = -1)
    {
        var revealMode = OptionGroupSingleton<DeputyOptions>.Instance.RevealDeputyUponShot.Value;
        if (revealMode == DeputyReveal.NoReveal)
        {
            Coroutines.Start(CustomTouMurderRpcs.CoAnimateDeath(targetVoteArea, associatedAnimKey));
        }
        else
        {
            ShotThisMeeting = true;
            DeputyKillAnimation.TargetId = targetVoteArea.PlayerId;
            if (revealMode is DeputyReveal.AnnounceRole)
            {
                DeputyKillAnimation.CreateKillAnimation(target.Data, target.Data, target.Data.DefaultOutfit.ColorId);
            }
            else
            {
                DeputyKillAnimation.CreateKillAnimation(source.Data, target.Data, source.Data.DefaultOutfit.ColorId);
                source.AddModifier<DeputyRevealedModifier>(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<DeputyRole>()));
                MeetingMenu.Instances.Do(x => x.HideSingle(source.PlayerId));
            }
            Coroutines.Start(CoStopShot());
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoStopShot()
    {
        yield return new WaitForSeconds(3);
        ShotThisMeeting = false;
    }
    private MeetingMenu meetingMenu;
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public PlayerControl? Killer { get; set; }
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string IdPart => "Deputy";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camp", "Camp"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camp.WikiDescription"),
                    TouCrewAssets.CampButtonSprite)
            ];
        }
    }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camp", "Camp"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camp.WikiDescription"),
                    TouCrewAssets.CampButtonSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shoot", "Shoot"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.SelectiveMeeting"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shoot.WikiDescription"),
                    TouAssets.ShootMeetingSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Deputy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public bool IsPowerCrew =>
        Killer || ModifierUtils.GetActiveModifiers<DeputyCampedModifier>()
            .Any(); // Only stop end game checks if the deputy can actually kill someone

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Deputy, "TouMira.Role.Crewmate.Deputy", 1.45f),
        Icon = TouRoleIcons.Deputy,
        OptionsScreenshot = TouBanners.DeputyRoleBanner,
        IntroSound = TouAudio.DeputyIntroSound,
    };

    public static void OnRoundStart()
    {
        CustomButtonSingleton<CampButton>.Instance.Usable = true;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                ClickGuess,
                MeetingAbilityType.Click,
                LegacyAssets.IsLegacy ? LegacyAssets.ShootMeetingSprite : TouAssets.ShootMeetingSprite,
                null!,
                IsExempt)
            {
                Position = new Vector3(-0.40f, 0f, -3f)
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        ShotThisMeeting = false;
        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
                Player.AmOwner && !Player.HasDied() && Killer != null && !Player.HasModifier<JailedModifier>());
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }

        Clear();
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void Clear()
    {
        var player = ModifierUtils.GetPlayersWithModifier<DeputyCampedModifier>(x => x.Deputy.AmOwner).FirstOrDefault();

        if (player != null && Player.AmOwner)
        {
            player.RpcRemoveModifier<DeputyCampedModifier>();
        }
    }

    public void ClickGuess(PlayerVoteArea voteArea, MeetingHud __)
    {
        var target = GameData.Instance.GetPlayerById(voteArea.PlayerId).Object;
        var role = Player.GetRole<DeputyRole>()!;

        if (role.Killer == target && !target.HasModifier<InvulnerabilityModifier>())
        {
            // Even though Deputy doesn't use the nameplate normally, it should grab it anyways incase Deputy isn't meant to reveal themselves
            Player.RpcMeetingMurder(target, MeetingAnimation.RoleSpecific, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                causeOfDeath: "Deputy");
        }
        else
        {
            var title =
                $"<color=#{TownOfUsColors.Deputy.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.DeputyMessageTitle")}</color>";
            var msg = MiraLocaleManager.Get("TownOfUsMira.Role.DeputyMissedShot");
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg, false, true);
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Deputy.ToTextColor()}{msg}</b></color>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Deputy);
            notif1.AdjustNotification();
        }

        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();
        }

        Clear();
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.PlayerId == Player.PlayerId || Player.Data.IsDead || voteArea!.AmDead ||
               voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true;
    }
}