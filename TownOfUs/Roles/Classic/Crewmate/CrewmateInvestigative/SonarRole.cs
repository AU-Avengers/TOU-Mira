using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SonarRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Sonar";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Track", "Track"),
                    MiraLocaleManager.Get($"TouRole{IdPart}TrackWikiDescription"),
                    TouCrewAssets.TrackSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Sonar;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Sonar.LoadAsset(), "TouMira.Role.Crewmate.Sonar", 1.45f),
        Icon = TouRoleIcons.Sonar,
        OptionsScreenshot = TouBanners.SonarRoleBanner,
        IntroSound = TouAudio.TrackerIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var players =
            LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.SonarTargetType.Value is SonarTargetStyle
                .Arrows
                ? ModifierUtils.GetPlayersWithModifier<SonarArrowTargetModifier>([HideFromIl2Cpp](x) =>
                    x.Owner == Player)
                : ModifierUtils.GetPlayersWithModifier<SonarHeartbeatTargetModifier>([HideFromIl2Cpp](x) =>
                    x.Owner == Player);

        var playerControls = players as PlayerControl[] ?? players.ToArray();
        if (playerControls.Length == 0)
        {
            return stringB;
        }

        stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{MiraLocaleManager.Get("TouRoleTrackerTrackedPlayers")}</b>");
        foreach (var plr in playerControls)
        {
            stringB.Append(TownOfUsPlugin.Culture, $"\n{plr.Data.PlayerName}");
        }

        return stringB;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void Clear()
    {
        var players =
            ModifierUtils.GetPlayersWithModifier<SonarArrowTargetModifier>([HideFromIl2Cpp](x) => x.Owner == Player);

        foreach (var player in players)
        {
            player.RemoveModifier<SonarArrowTargetModifier>();
        }

        players =
            ModifierUtils.GetPlayersWithModifier<SonarHeartbeatTargetModifier>([HideFromIl2Cpp](x) => x.Owner == Player);

        foreach (var player in players)
        {
            player.RemoveModifier<SonarHeartbeatTargetModifier>();
        }
    }
}