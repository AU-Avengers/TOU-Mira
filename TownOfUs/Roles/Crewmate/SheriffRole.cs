using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SheriffRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public bool HasMisfired { get; set; }
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Sheriff";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Shoot", "Shoot"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ShootWikiDescription"),
                    TouCrewAssets.SheriffShootSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Sheriff;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => !HasMisfired; // Always disable end game checks if the sheriff hasn't misfired

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Sheriff,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{RoleColor.ToTextColor()}{TouLocale.Get("YouAreA")}<b> {RoleName}.</b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(RoleAlignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{TouLocale.GetParsed($"TouRole{LocaleKey}TabDescriptionEgo")}");
        }
        else
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{RoleLongDescription}");
            var addedText = "d";
            if (!CustomButtonSingleton<SheriffShootButton>.Instance.FailedShot)
            {
                var missType = OptionGroupSingleton<SheriffOptions>.Instance.MisfireType;
                addedText = $"Kills{missType}";
            }
            stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{TouLocale.GetParsed($"TouRole{LocaleKey}TabMisfire{addedText}")}</b>");
        }

        return stringB;
    }

    public static void OnRoundStart()
    {
        CustomButtonSingleton<SheriffShootButton>.Instance.Usable = true;
    }

    [MethodRpc((uint)TownOfUsRpc.SheriffMisfire)]
    public static void RpcSheriffMisfire(PlayerControl sheriff)
    {
        if (sheriff.Data.Role is not SheriffRole role)
        {
            Error("RpcSheriffMisfire - Invalid sheriff");
            return;
        }

        role.HasMisfired = true;

        if (GameHistory.PlayerStats.TryGetValue(sheriff.PlayerId, out var stats))
        {
            stats.IncorrectKills += 1;
        }
    }
}