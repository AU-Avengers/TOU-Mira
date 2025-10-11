using System.Globalization;
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

    public static StringBuilder SetNewTabText(ICustomRole role)
    {
        var alignment = MiscUtils.GetRoleAlignment(role);

        var youAre = "Your role is";
        if (role is ITownOfUsRole touRole2)
        {
            youAre = touRole2.YouAreText;
        }

        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }

    public static StringBuilder SetDeadTabText(ICustomRole role)
    {
        var alignment = MiscUtils.GetRoleAlignment(role);

        var youAre = "Your role was";
        if (role is ITownOfUsRole touRole2)
        {
            youAre = touRole2.YouWereText;
        }

        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return SetNewTabText(this);
    }
}

public enum RoleAlignment
{
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
    GameOutlier // I honestly have no idea what else to put here lol
}