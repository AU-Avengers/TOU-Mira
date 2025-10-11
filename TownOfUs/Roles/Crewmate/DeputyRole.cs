using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class DeputyRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu meetingMenu;
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public PlayerControl? Killer { get; set; }
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Deputy";
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
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Camp", "Camp"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CampWikiDescription"),
                    TouCrewAssets.CampButtonSprite)
            };
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
        Icon = TouRoleIcons.Deputy,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

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
                TouAssets.ShootMeetingSprite,
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

        if (Player.AmOwner)
        {
            meetingMenu.GenButtons(MeetingHud.Instance,
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
        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;
        var role = Player.GetRole<DeputyRole>()!;

        if (role.Killer == target && !target.HasModifier<InvulnerabilityModifier>())
        {
            Player.RpcCustomMurder(target, createDeadBody: false, teleportMurderer: false);
        }
        else
        {
            var title =
                $"<color=#{TownOfUsColors.Deputy.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleDeputyMessageTitle")}</color>";
            var msg = TouLocale.Get("TouRoleDeputyMissedShot");
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg, false, true);
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Deputy.ToTextColor()}{msg}</b></color>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Deputy.LoadAsset());
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
        return voteArea?.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || voteArea!.AmDead ||
               voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true;
    }
}