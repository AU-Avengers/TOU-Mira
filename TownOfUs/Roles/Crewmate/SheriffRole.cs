using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Game;
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
    public string RoleName => TouLocale.Get(TouNames.Sheriff, "Sheriff");
    public string RoleDescription => "Shoot The <color=#FF0000FF>Impostor</color>";
    public string RoleLongDescription => "Kill off the impostors but don't kill crewmates";
    public Color RoleColor => TownOfUsColors.Sheriff;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => !HasMisfired; // Always disable end game checks if the sheriff hasn't misfired

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Sheriff,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var missType = OptionGroupSingleton<SheriffOptions>.Instance.MisfireType;

        if (CustomButtonSingleton<SheriffShootButton>.Instance.FailedShot)
        {
            stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>You can no longer shoot.</b>");
        }
        else
        {
            switch (missType)
            {
                case MisfireOptions.Both:
                    stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>Misfiring kills you and your target.</b>");
                    break;
                case MisfireOptions.Sheriff:
                    stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>Misfiring will lead to suicide.</b>");
                    break;
                case MisfireOptions.Target:
                    stringB.AppendLine(CultureInfo.InvariantCulture,
                        $"<b>Misfiring will lead to your target's death,\nat the cost of your ability.</b>");
                    break;
                default:
                    stringB.AppendLine(CultureInfo.InvariantCulture,
                        $"<b>Misfiring will prevent you from shooting again.</b>");
                    break;
            }
        }

        if (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>You may shoot without repercussions.</b>");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Killing that can shoot a player to attempt to kill them. If {RoleName} doesn't die to misfire, they will lose the ability to shoot." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Shoot",
            "Shoot a player to kill them, misfiring if they aren't a Impostor or one of the other selected shootable factions",
            TouCrewAssets.SheriffShootSprite)
    ];

    public static void OnRoundStart()
    {
        CustomButtonSingleton<SheriffShootButton>.Instance.Usable = true;
    }

    [MethodRpc((uint)TownOfUsRpc.SheriffMisfire, SendImmediately = true)]
    public static void RpcSheriffMisfire(PlayerControl sheriff)
    {
        if (sheriff.Data.Role is not SheriffRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcSheriffMisfire - Invalid sheriff");
            return;
        }

        role.HasMisfired = true;

        if (GameHistory.PlayerStats.TryGetValue(sheriff.PlayerId, out var stats))
        {
            stats.IncorrectKills += 1;
        }
    }
}