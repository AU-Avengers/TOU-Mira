using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class GuardianRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public RoleTypes? ProtectedRole { get; set; }
    public bool ProtectedRoleExists { get; set; }
    public List<RoleTypes> AegisAttacked { get; } = [];
    public bool UsedOnGuardian { get; private set; }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string RoleName => "Guardian";
    public string RoleDescription => "Cast Aegis over roles";
    public string RoleLongDescription => "Protect all players with a role of your choice.";
    public Color RoleColor => TownOfUsColors.Guardian;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Guardian
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (ProtectedRole.HasValue)
        {
            Color roleColor;
            string roleName;
            var role = RoleManager.Instance.GetRole(ProtectedRole.Value);
            if (role is ICustomRole customRole)
            {
                roleColor = customRole.RoleColor;
                roleName = customRole.RoleName;
            }
            else
            {
                roleColor = role.TeamColor;
                roleName = role.NiceName;
            }
            
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>Aegis: </b>{roleColor.ToTextColor()}{roleName}</color>");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Protective role that can cast Aegis over any role to protect players."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Aegis",
            "Cast an Aegis over any valid role. All players with that role will be protected for the following round. During a meeting, you will learn if your Aegis protected anyone and if they were attacked. You can Aegis the Guardian role once",
            TouCrewAssets.FortifySprite)
    ];

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void OpenAegisMenu()
    {
        var menu = GuesserMenu.Create();
        menu.Begin(IsRoleValid, ClickRoleHandle);

        void ClickRoleHandle(RoleBehaviour role)
        {
            menu.Close();
            RpcGuardianAegis(Player, role.Role);
            CustomButtonSingleton<GuardianAegisButton>.Instance.SetTimer(OptionGroupSingleton<GuardianOptions>.Instance.AegisCooldown);
        }
    }

    private bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead || role is IGhostRole)
        {
            return false;
        }

        if (role is GuardianRole)
        {
            return !UsedOnGuardian;
        }

        if (role.Role == ProtectedRole)
        {
            return false;
        }

        bool isEvil = role is ICustomRole customRole ? customRole.Team != ModdedRoleTeams.Crewmate : role.IsImpostor();
        if (isEvil)
        {
            return Player.HasModifier<EgotistModifier>();
        }

        return true;
    }

    public void SelectAegis(RoleTypes role)
    {
        Clear();
        ProtectedRole = role;
        ProtectedRoleExists = false;

        if (role == (RoleTypes)RoleId.Get<GuardianRole>())
        {
            UsedOnGuardian = true;
        }
        
        foreach (var player in Helpers.GetAlivePlayers())
        {
            if (player.Data.Role.Role == ProtectedRole)
            {
                ProtectedRoleExists = true;
                player.AddModifier<GuardianAegisModifier>(Player);
            }
        }
    }

    public void Clear()
    {
        ProtectedRole = null;
        ModifierUtils.GetActiveModifiers<GuardianAegisModifier>()
            .Where(x => x.Guardian == Player)
            .Do(x => Player.RemoveModifier(x));
    }
    
    [MethodRpc((uint)TownOfUsRpc.GuardianAegis)]
    public static void RpcGuardianAegis(PlayerControl player, RoleTypes role)
    {
        if (player.Data.Role is not GuardianRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcGuardianAegis - Invalid guardian");
            return;
        }

        var guardian = player.GetRole<GuardianRole>();
        guardian?.SelectAegis(role);
    }
    
    [MethodRpc((uint)TownOfUsRpc.GuardianAegisAttacked)]
    public static void RpcGuardianAegisAttacked(PlayerControl player, PlayerControl source, PlayerControl target)
    {
        if (player.Data.Role is not GuardianRole guardian)
        {
            Logger<TownOfUsPlugin>.Error("RpcGuardianAegis - Invalid guardian");
            return;
        }
        if (!target.HasModifier<GuardianAegisModifier>())
        {
            Logger<TownOfUsPlugin>.Error("RpcGuardianAegis - Target has no Aegis");
            return;
        }

        var targetRole = target.Data.Role.Role;
        if (guardian.ProtectedRole != targetRole)
        {
            return;
        }

        if (!guardian.AegisAttacked.Contains(targetRole))
        {
            guardian.AegisAttacked.Add(targetRole);
        }
    }
}