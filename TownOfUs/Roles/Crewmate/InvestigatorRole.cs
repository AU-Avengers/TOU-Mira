using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class InvestigatorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string RoleName => TouLocale.Get(TouNames.Investigator, "Investigator");
    public string RoleDescription => "Find All Impostors By Examining Footprints.";
    public string RoleLongDescription => "You can see everyone's footprints.";
    public Color RoleColor => TownOfUsColors.Investigator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Investigator,
        IntroSound = TouAudio.SpyIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Investigative role can see player's footprints throughout the game. Swooped players' footprints will not be visible to the {RoleName}."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (!player.AmOwner)
        {
            return;
        }

        Helpers.GetAlivePlayers().Where(plr => !plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().AddModifier<FootstepsModifier>());
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (!targetPlayer.AmOwner)
        {
            return;
        }

        PlayerControl.AllPlayerControls.ToArray().Where(plr => plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().RemoveModifier<FootstepsModifier>());
    }

    public override void OnDeath(DeathReason reason)
    {
        if (!Player.AmOwner)
        {
            return;
        }

        PlayerControl.AllPlayerControls.ToArray().Where(plr => plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().RemoveModifier<FootstepsModifier>());
    }
}