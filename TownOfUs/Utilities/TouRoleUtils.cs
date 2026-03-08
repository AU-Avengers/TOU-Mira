using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
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
        var playerTask = playerControl.myTasks.ToArray().FirstOrDefault(t => t.name == "NeutralRoleText");
        if (playerTask == null)
        {
            playerTask = playerControl.myTasks.ToArray().FirstOrDefault(t => t.name == "ImpostorRole");
        }
        if (playerTask != null)
        {
            playerControl.myTasks.Remove(playerTask);
            playerTask.gameObject.Destroy();
        }
    }

    public static Sprite GetRoleIcon(this RoleBehaviour role)
    {
        var roleImg = GetBasicRoleIcon(role);
        var customRole = role as ICustomRole;
        if (customRole != null && customRole.Configuration.Icon != null)
        {
            roleImg = customRole.Configuration.Icon.LoadAsset();
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
            return TouRoleIcons.Impostor.LoadAsset();
        }
        if (basicText.Contains("Crewmate"))
        {
            return TouRoleIcons.Crewmate.LoadAsset();
        }
        return TouRoleIcons.Neutral.LoadAsset();
    }

    public static Sprite GetBasicRoleIcon(RoleBehaviour role)
    {
        if (role.IsImpostor())
        {
            return TouRoleIcons.Impostor.LoadAsset();
        }
        if (role.IsCrewmate())
        {
            return TouRoleIcons.Crewmate.LoadAsset();
        }
        return TouRoleIcons.Neutral.LoadAsset();
    }

    public static Sprite? TryGetVanillaRoleIcon(RoleTypes roleType)
    {
        return roleType switch
        {
            RoleTypes.GuardianAngel => TouRoleIcons.GuardianAngel.LoadAsset(),
            RoleTypes.Detective => TouRoleIcons.Detective.LoadAsset(),
            RoleTypes.Tracker => TouRoleIcons.Tracker.LoadAsset(),
            RoleTypes.Scientist => TouRoleIcons.Scientist.LoadAsset(),
            RoleTypes.Noisemaker => TouRoleIcons.Noisemaker.LoadAsset(),
            RoleTypes.Phantom => TouRoleIcons.Phantom.LoadAsset(),
            RoleTypes.Shapeshifter => TouRoleIcons.Shapeshifter.LoadAsset(),
            RoleTypes.Viper => TouRoleIcons.Viper.LoadAsset(),
            _ => null
        };
    }

    public static bool CanGetGhostRole(this PlayerControl player)
    {
        return !player.HasModifier<BasicGhostModifier>()
            && player.Data.Role is not SpectatorRole
            && player.Data.Role is not GuardianAngelRole
            && player.Data.Role is not IGhostRole;
    }
    public static bool AreTeammates(PlayerControl player, PlayerControl other)
    {
        var playerRole = player.GetRoleWhenAlive();
        var otherRole = other.GetRoleWhenAlive();
        var flag = (player.IsImpostorAligned() && other.IsImpostorAligned()) ||
                   playerRole.Role == otherRole.Role ||
                   (player.IsLover() && other.IsLover());
        return flag;
    }

    public static bool CanKill(PlayerControl player)
    {
        var canBetray = PlayerControl.LocalPlayer.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates;

        return !(AreTeammates(PlayerControl.LocalPlayer, player) && canBetray && !player.IsLover());
    }
    public static string GetRoleLocaleKey(this RoleBehaviour role)
    {
        var touRole = role as ITownOfUsRole;
        if (touRole != null && touRole.LocaleKey != "KEY_MISS")
        {
            return touRole.LocaleKey;
        }

        if (!role.IsCustomRole())
        {
            return role.Role.ToString();
        }

        return role.GetRoleName().Replace(" ", "");
    }
    public static bool IsRevealed(this PlayerControl? player) =>
        player?.GetModifiers<BaseRevealModifier>().Any(x => x.Visible && x.RevealRole) == true ||
        player?.Data?.Role is MayorRole mayor && mayor.Revealed;
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
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
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
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"{role.RoleLongDescription}");

        return stringB;
    }

    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);

    public static Vent? GetClosestUsableVent
    {
        get
        {
            var player = PlayerControl.LocalPlayer;
            Vector2 truePosition = player.GetTruePosition();
            var closestVents = Helpers.GetNearestObjectsOfType<Vent>(truePosition, player.MaxReportDistance, Filter);
            Vent? vent = null;
            if (closestVents.Count == 0)
            {
                return null;
            }
            foreach (var closeVent in closestVents)
            {
                if (player.CanUseVent(closeVent))
                {
                    vent = closeVent;
                    break;
                }
            }
            return vent;
        }
    }
}
