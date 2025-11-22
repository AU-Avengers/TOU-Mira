using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using TownOfUs.Utilities;

namespace TownOfUs.Roles;

public interface ITownOfUsRole : ICustomRole
{
    RoleAlignment RoleAlignment { get; }

    bool HasImpostorVision => false;
    public virtual bool MetWinCon => false;
    public virtual string LocaleKey => "KEY_MISS";
    public static Dictionary<string, string> LocaleList => [];

    public CustomRoleConfiguration Configuration => new(this)
    {
        /*HideSettings = MiscUtils.CurrentGamemode() is not TouGamemode.Normal*/
    };

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
                LocaleKey.StartsWith("the", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "";
            }

            return TouLocale.Get($"YouAre{prefix}");
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
                LocaleKey.StartsWith("the", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "";
            }

            return TouLocale.Get($"YouWere{prefix}");
        }
    }

    RoleOptionsGroup ICustomRole.RoleOptionsGroup
    {
        get
        {
            if (RoleAlignment == RoleAlignment.CrewmateInvestigative)
            {
                return TouRoleGroups.CrewInvest;
            }

            if (RoleAlignment == RoleAlignment.CrewmateKilling)
            {
                return TouRoleGroups.CrewKiller;
            }

            if (RoleAlignment == RoleAlignment.CrewmateProtective)
            {
                return TouRoleGroups.CrewProc;
            }

            if (RoleAlignment == RoleAlignment.CrewmatePower)
            {
                return TouRoleGroups.CrewPower;
            }

            if (RoleAlignment == RoleAlignment.ImpostorConcealing)
            {
                return TouRoleGroups.ImpConceal;
            }

            if (RoleAlignment == RoleAlignment.ImpostorKilling)
            {
                return TouRoleGroups.ImpKiller;
            }

            if (RoleAlignment == RoleAlignment.ImpostorPower)
            {
                return TouRoleGroups.ImpPower;
            }

            if (RoleAlignment == RoleAlignment.NeutralEvil)
            {
                return TouRoleGroups.NeutralEvil;
            }

            if (RoleAlignment == RoleAlignment.NeutralOutlier)
            {
                return TouRoleGroups.NeutralOutlier;
            }

            if (RoleAlignment == RoleAlignment.NeutralKilling)
            {
                return TouRoleGroups.NeutralKiller;
            }

            if (RoleAlignment == RoleAlignment.CrewmateHider)
            {
                return TouRoleGroups.CrewHider;
            }

            if (RoleAlignment == RoleAlignment.ImpostorSeeker)
            {
                return TouRoleGroups.ImpSeeker;
            }

            if (RoleAlignment == RoleAlignment.ImpostorCultist || RoleAlignment == RoleAlignment.ImpostorRecruit)
            {
                return TouRoleGroups.ImpCultist;
            }

            if (RoleAlignment == RoleAlignment.CrewmateBeliever)
            {
                return TouRoleGroups.CrewBeliever;
            }

            if (RoleAlignment == RoleAlignment.CrewmateObstinate)
            {
                return TouRoleGroups.CrewObstinate;
            }

            if (RoleAlignment == RoleAlignment.NeutralObstinate)
            {
                return TouRoleGroups.NeutralObstinate;
            }

            if (RoleAlignment == RoleAlignment.GameOutlier)
            {
                return TouRoleGroups.Other;
            }

            return Team switch
            {
                ModdedRoleTeams.Crewmate => TouRoleGroups.CrewSup,
                ModdedRoleTeams.Impostor => TouRoleGroups.ImpSup,
                _ => TouRoleGroups.NeutralBenign
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
    StringBuilder SetTabText()
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
    // Hide and Seek Alignments
    CrewmateHider,
    ImpostorSeeker,
    // Cultist Alignments
    ImpostorCultist,
    ImpostorRecruit,
    CrewmateBeliever,
    CrewmateObstinate,
    NeutralObstinate,
}