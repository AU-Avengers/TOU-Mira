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
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SheriffRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public bool HasMisfired { get; set; }
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string IdPart => "Sheriff";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

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
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shoot", "Shoot"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shoot.WikiDescription"),
                    TouCrewAssets.SheriffShootSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Sheriff;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => !HasMisfired; // Always disable end game checks if the sheriff hasn't misfired

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Sheriff.LoadAsset(), "TouMira.Role.Crewmate.Sheriff", 1.45f),
        Icon = TouRoleIcons.Sheriff,
        OptionsScreenshot = TouBanners.SheriffRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{RoleColor.ToTextColor()}{MiraLocaleManager.Get("YouAreA")}<b> {RoleName}.</b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{MiraLocaleManager.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(RoleAlignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}TabDescriptionEgo")}");
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
            stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}TabMisfire{addedText}")}</b>");
        }

        return stringB;
    }

    [MethodRpc((uint)TownOfUsRpc.SheriffMisfire)]
    public static void RpcSheriffMisfire(PlayerControl sheriff)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(sheriff);
            return;
        }
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