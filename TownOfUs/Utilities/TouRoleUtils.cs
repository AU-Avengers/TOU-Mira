using System.Text;
using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class TouRoleUtils
{
    public static void ClearTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }

        var playerTask = playerControl.myTasks.FirstOrDefault(x => x is ImportantTextTask);

        if (playerTask != null)
        {
            playerControl.myTasks.Remove(playerTask);
            playerTask.gameObject.DeepDestroy();
        }
    }

    public static Sprite GetRoleIcon(this RoleBehaviour role)
    {
        var roleImg = GetBasicRoleIcon(role);
        if (role is ICustomRole { Configuration.Icon: { } icon })
        {
            roleImg = icon;
        }
        else if (role.RoleIconSolid != null)
        {
            roleImg = role.RoleIconSolid;
            var changedIcon = TryGetVanillaRoleIcon(role.Role);
            if (changedIcon != null)
            {
                roleImg = changedIcon;
            }
        }

        return roleImg;
    }

    public static Sprite GetBasicRoleIcon(ITownOfUsRole role)
    {
        var basicText = role.RoleAlignment.ToString();
        if (basicText.Contains("Impostor"))
        {
            return TouRoleIcons.Impostor;
        }

        if (basicText.Contains("Crewmate"))
        {
            return TouRoleIcons.Crewmate;
        }

        return TouRoleIcons.Neutral;
    }

    public static Sprite GetBasicRoleIcon(RoleBehaviour role)
    {
        if (role.IsImpostor())
        {
            return TouRoleIcons.Impostor;
        }

        if (role.IsCrewmate())
        {
            return TouRoleIcons.Crewmate;
        }

        return TouRoleIcons.Neutral;
    }

    public static LoadableAsset<Sprite>? TryGetVanillaRoleIcon(RoleTypes roleType)
    {
        return roleType switch
        {
            RoleTypes.GuardianAngel => TouRoleIcons.GuardianAngel,
            RoleTypes.Detective => TouRoleIcons.Detective,
            RoleTypes.Tracker => TouRoleIcons.Tracker,
            RoleTypes.Scientist => TouRoleIcons.Scientist,
            RoleTypes.Noisemaker => TouRoleIcons.Noisemaker,
            RoleTypes.Phantom => TouRoleIcons.Phantom,
            RoleTypes.Shapeshifter => TouRoleIcons.Shapeshifter,
            RoleTypes.Engineer => TouRoleIcons.Engineer,
            RoleTypes.Judge => TouRoleIcons.Prosecutor,
            RoleTypes.Viper => TouRoleIcons.Viper,
            RoleTypes.SpiritGuide => TouRoleIcons.Spectator,
            _ => null
        };
    }

    public static bool CanGetGhostRole(this PlayerControl player)
    {
        return !player.HasModifier<BasicGhostModifier>()
               && player.Data.Role is not SpectatorRole and not IGhostRole
               && player.Data.Role.Role is not RoleTypes.GuardianAngel and not RoleTypes.SpiritGuide;
    }

    public static bool AreTeammates(PlayerControl player, PlayerControl other, bool impFfa, bool impsSeeRole, out bool canSeeRoles, out bool canSeeColor)
    {
        var playerRole = player.GetRoleWhenAlive();
        var otherRole = other.GetRoleWhenAlive();
        var consideredTeammates = false;
        var visible = otherRole is IVisibleRole vis && vis.CanOtherRoleSee(playerRole, out consideredTeammates);
        var areImps = player.IsImpostorAligned() && other.IsImpostorAligned();
        var areLovers = player.IsLoverWithPlayer(other);
        var flag = areImps || consideredTeammates || areLovers;
        canSeeRoles = areImps && !impFfa && impsSeeRole || visible;
        canSeeColor = areImps && !impFfa || visible;
        return flag;
    }

    /*public static bool CanKill(PlayerControl player)
    {
        var canBetray = PlayerControl.LocalPlayer.IsLover() &&
                        OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates;

        return !(AreTeammates(PlayerControl.LocalPlayer, player) && canBetray && !player.IsLover());
    }*/

    public static string GetRoleIdPart(this RoleBehaviour role)
    {
        if (role is ICustomRole { IdPart: not "KEY_MISS" and var idPart })
        {
            return idPart;
        }

        if (!role.IsCustomRole())
        {
            return role.Role.ToString();
        }

        return role.GetRoleName().Replace(" ", "");
    }

    public static bool IsRevealed(this PlayerControl? player) =>
        player?.HasModifier<BaseRevealModifier>(x => x.Visible && x.RevealRole) == true ||
        player?.Data?.Role is MayorRole { Revealed: true };

    public static StringBuilder SetTabText(ICustomRole role)
    {
        var alignment = MiscUtils.GetRoleAlignment(role);

        var youAre = "Your role is";
        if (role is ITownOfUsRole touRole2)
        {
            youAre = touRole2.YouAreText;
        }

        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.‎ ‎ ‎ </b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{MiraLocaleManager.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"{role.RoleLongDescription}");

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
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.‎ ‎ ‎ </b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{MiraLocaleManager.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"{role.RoleLongDescription}");

        return stringB;
    }
}
