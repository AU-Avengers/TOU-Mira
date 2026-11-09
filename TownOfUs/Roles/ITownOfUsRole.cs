using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TMPro;
using UnityEngine;

namespace TownOfUs.Roles;

public interface ITownOfUsRole : ICustomRole
{
    GameObject ICustomRole.GetAdvancedWiki(MatchInfoGuide guide, TextMeshPro titleText, SpriteRenderer titleIcon, Scroller parent)
    {
        var abilities = WikiAbilities;
        if (abilities.Count == 0 && this is IWikiDiscoverable { Abilities: { Count: > 0 } wiki })
        {
            abilities = wiki.Select(a => new AdvancedWikiAbilityDescription(
                a.Name,
                MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
                a.Description,
                a.Icon)).ToList();
        }

        return Helpers.CreateAdvancedWikiPage(
            guide,
            titleText,
            titleIcon,
            parent,
            RoleNameLocale,
            $"{RoleName} ({RoleFactionTitle})",
            Configuration.Icon ?? MiraAssets.IconNeutral,
            RoleWikiDescription,
            abilities);
    }

    /// <summary>
    /// Runs when all roles are finished up being added. Mostly used to set up icons.
    /// </summary>
    public virtual void InitialSetup()
    {
        // Does nothing by default
    }
    RoleAlignment RoleAlignment { get; }

    bool HasImpostorVision => false;
    public virtual bool MetWinCon => false;
    public bool IsDraftable => true;
    public static Dictionary<string, string> LocaleList => [];
    string ICustomRole.IdPrefix => "TownOfUsMira.Role";

    [HideFromIl2Cpp]
    public virtual bool CanModifierContinueGame(BaseModifier modifier)
    {
        return false;
    }
    string ICustomRole.RoleFactionTitle => MiscUtils.GetParsedRoleAlignment(RoleAlignment);

    string? ICustomRole.GetCustomEjectionMessage(NetworkedPlayerInfo player)
    {
        var prefix = "A";
        if (RoleName.StartsWithVowel())
        {
            prefix = "An";
        }

        if (Configuration.MaxRoleCount is 0 or 1)
        {
            prefix = "The";
        }

        if (RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase) ||
            IdPart.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            prefix = "";
        }
        return MiraLocaleManager.Get($"ExileTextConfirm{prefix}").Replace("<player>", player.PlayerName).Replace("<role>", RoleName);
    }

    public virtual string YouAreText
    {
        get
        {
            var prefix = "A";
            if (RoleName.StartsWithVowel())
            {
                prefix = "An";
            }

            if (Configuration.MaxRoleCount is 0 or 1)
            {
                prefix = "The";
            }

            if (RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase) ||
                IdPart.StartsWith("the", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "";
            }

            return MiraLocaleManager.Get($"YouAre{prefix}");
        }
    }

    public virtual string YouWereText
    {
        get
        {
            var prefix = "A";
            if (RoleName.StartsWithVowel())
            {
                prefix = "An";
            }

            if (Configuration.MaxRoleCount is 0 or 1)
            {
                prefix = "The";
            }

            if (RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase) ||
                IdPart.StartsWith("the", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "";
            }

            return MiraLocaleManager.Get($"YouWere{prefix}");
        }
    }

    RoleOptionsGroup ICustomRole.RoleOptionsGroup
    {
        get
        {
            return RoleAlignment switch
            {
                RoleAlignment.CrewmateInvestigative => TouRoleGroups.CrewInvest,
                RoleAlignment.CrewmateKilling       => TouRoleGroups.CrewKiller,
                RoleAlignment.CrewmateProtective    => TouRoleGroups.CrewProc,
                RoleAlignment.CrewmatePower         => TouRoleGroups.CrewPower,
                RoleAlignment.ImpostorConcealing    => TouRoleGroups.ImpConceal,
                RoleAlignment.ImpostorKilling       => TouRoleGroups.ImpKiller,
                RoleAlignment.ImpostorPower         => TouRoleGroups.ImpPower,
                RoleAlignment.NeutralEvil           => TouRoleGroups.NeutralEvil,
                RoleAlignment.NeutralOutlier        => TouRoleGroups.NeutralOutlier,
                RoleAlignment.NeutralKilling        => TouRoleGroups.NeutralKiller,
                RoleAlignment.CrewmateHider         => TouRoleGroups.CrewHider,
                RoleAlignment.ImpostorSeeker        => TouRoleGroups.ImpSeeker,
                RoleAlignment.ImpostorCultist       => TouRoleGroups.ImpCultist,
                RoleAlignment.ImpostorFollower      => TouRoleGroups.ImpFollower,
                RoleAlignment.CrewmateBeliever      => TouRoleGroups.CrewBeliever,
                RoleAlignment.CrewmateObstinate     => TouRoleGroups.CrewObstinate,
                RoleAlignment.NeutralObstinate      => TouRoleGroups.NeutralObstinate,
                RoleAlignment.CrewmateAfterlife     => TouRoleGroups.CrewAfterlife,
                RoleAlignment.NeutralAfterlife      => TouRoleGroups.NeutralAfterlife,
                RoleAlignment.ImpostorAfterlife     => TouRoleGroups.ImpAfterlife,
                RoleAlignment.CrewmateGhost         => TouRoleGroups.CrewGhost,
                RoleAlignment.NeutralGhost          => TouRoleGroups.NeutralGhost,
                RoleAlignment.ImpostorGhost         => TouRoleGroups.ImpGhost,
                RoleAlignment.GameOutlier           => TouRoleGroups.Other,
                RoleAlignment.FrenzyKiller          => TouRoleGroups.FrenzyKiller,
                _ => Team switch
                {
                    ModdedRoleTeams.Crewmate => TouRoleGroups.CrewSup,
                    ModdedRoleTeams.Impostor => TouRoleGroups.ImpSup,
                    _                        => TouRoleGroups.NeutralBenign
                }
            };
        }
    }

    bool WinConditionMet()
    {
        return false;
    }

    /// <summary>
    ///     LobbyStart - Called for each role when a lobby begins.
    /// </summary>
    void LobbyStart()
    {
    }

    /// <summary>
    ///     OffsetButtons - Called when the role initializes and when the player offsets their buttons even without a vent button.
    /// </summary>
    void OffsetButtons()
    {
    }

    public static StringBuilder SetNewTabText(ICustomRole role)
    {
        return TouRoleUtils.SetTabText(role);
    }

    public static StringBuilder SetDeadTabText(ICustomRole role)
    {
        return TouRoleUtils.SetDeadTabText(role);
    }

    [HideFromIl2Cpp]
    StringBuilder ICustomRole.SetTabText()
    {
        return SetNewTabText(this);
    }
}

public enum RoleAlignment
{
    // Base Town of Us Alignments
    CrewmateInvestigative,
    CrewmateKilling,
    CrewmateProtective,
    CrewmatePower,
    CrewmateSupport,
    ImpostorConcealing,
    ImpostorKilling,
    ImpostorPower,
    ImpostorSupport,
    NeutralBenign,
    NeutralEvil,
    NeutralOutlier,
    NeutralKilling,
    GameOutlier, // I honestly have no idea what else to put here
    CrewmateGhost,
    ImpostorGhost,
    NeutralGhost,
    Crewmate,
    Impostor,
    Neutral,
    CrewmateAfterlife,
    ImpostorAfterlife,
    NeutralAfterlife,
    // Hide and Seek Alignments
    CrewmateHider,
    ImpostorSeeker,
    // Cultist Alignments
    ImpostorCultist,
    ImpostorFollower,
    CrewmateBeliever,
    CrewmateObstinate,
    NeutralObstinate,
    // Killing Frenzy
    FrenzyKiller
}