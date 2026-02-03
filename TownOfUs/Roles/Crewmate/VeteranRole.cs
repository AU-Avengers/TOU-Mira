using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class VeteranRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public int Alerts { get; set; }
    public bool AttackedRecently { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Veteran";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Alert", "Alert"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}AlertWikiDescription"),
                    TouCrewAssets.AlertSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Veteran;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => Alerts > 0; // Stop end game checks if the veteran can still alert

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Veteran,
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
        if (!OptionGroupSingleton<VeteranOptions>.Instance.KnowWhenAttackedInMeeting.Value)
        {
            return;
        }
        var title = $"<color=#{TownOfUsColors.Veteran.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleVeteranMessageTitle")}</color>";
        var msg = TouLocale.GetParsed("TouRoleVeteranAttackMessage");

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{msg}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Veteran.LoadAsset());

        notif1.AdjustNotification();

        MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg, false, true);
    }

    [MethodRpc((uint)TownOfUsRpc.RecentVetAttack)]
    public static void RpcRecentVetAttack(PlayerControl veteran)
    {
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