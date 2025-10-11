using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TrackerTouRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string LocaleKey => "Tracker";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Track", "Track"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}TrackWikiDescription"),
                    TouCrewAssets.TrackSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Tracker;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Tracker,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Tracker)
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var players =
            ModifierUtils.GetPlayersWithModifier<TrackerArrowTargetModifier>([HideFromIl2Cpp](x) => x.Owner == Player);

        var playerControls = players as PlayerControl[] ?? players.ToArray();
        if (playerControls.Length == 0)
        {
            return stringB;
        }

        stringB.Append("\n<b>Tracked Players:</b>");
        foreach (var plr in playerControls)
        {
            stringB.Append(CultureInfo.InvariantCulture, $"\n{plr.Data.PlayerName}");
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
            ModifierUtils.GetPlayersWithModifier<TrackerArrowTargetModifier>([HideFromIl2Cpp](x) => x.Owner == Player);

        foreach (var player in players)
        {
            player.RemoveModifier<TrackerArrowTargetModifier>();
        }
    }
}