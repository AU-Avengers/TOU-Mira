using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class VeteranRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public int Alerts { get; set; }
    public bool AttackedRecently { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Veteran";

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
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Alert", "Alert"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Alert.WikiDescription"),
                    TouCrewAssets.AlertSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Veteran;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => Alerts > 0; // Stop end game checks if the veteran can still alert

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Veteran.LoadAsset(), "TouMira.Role.Crewmate.Veteran", 1.45f),
        Icon = TouRoleIcons.Veteran,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (!AttackedRecently)
        {
            return;
        }
        AttackedRecently = false;
        if (!OptionGroupSingleton<VeteranOptions>.Instance.KnowWhenAttackedInMeeting.Value || !Player.AmOwner)
        {
            return;
        }
        var title = $"<color=#{TownOfUsColors.Veteran.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.VeteranMessageTitle")}</color>";
        var msg = MiraLocaleManager.Get("TownOfUsMira.Role.VeteranAttackMessage");

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{msg}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Veteran.LoadAsset());

        notif1.AdjustNotification();

        MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg, false, true);
    }

    [MethodRpc((uint)TownOfUsRpc.RecentVetAttack)]
    public static void RpcRecentVetAttack(PlayerControl veteran)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(veteran);
            return;
        }
        if (veteran.Data.Role is not VeteranRole role)
        {
            Error("RpcRecentVetAttack - Invalid veteran");
            return;
        }
        role.AttackedRecently = true;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Alerts = (int)OptionGroupSingleton<VeteranOptions>.Instance.MaxNumAlerts;
    }
}