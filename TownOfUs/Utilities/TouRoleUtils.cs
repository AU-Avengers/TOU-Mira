using System.Text;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Patches;
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

        var playerTask = playerControl.myTasks.ToArray().FirstOrDefault(x => x is ImportantTextTask);

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
        var canBetray = PlayerControl.LocalPlayer.IsLover() &&
                        OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates;

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

    public static Vent? GetClosestUsableVent(bool forVenting, float distance)
    {
        var playerControl = PlayerControl.LocalPlayer;
        var data = playerControl.Data;
        Vector2 truePosition = playerControl.GetTruePosition();
        var flag2 = (playerControl.CanMove || playerControl.inVent || !forVenting);
        int num2 = Physics2D.OverlapCircleNonAlloc(truePosition, distance, playerControl.hitBuffer, Constants.Usables);
        float num3 = float.MaxValue;
        List<Vent> list = new List<Vent>();
        for (int i = 0; i < num2; i++)
        {
            Collider2D collider2D = playerControl.hitBuffer[i];
            if (!playerControl.cache.TryGetValue(collider2D, out var array))
            {
                playerControl.cache[collider2D] = collider2D.GetComponents<IUsable>().ToArray();
                array = playerControl.cache[collider2D];
            }

            if (array != null && flag2)
            {
                foreach (var usable2 in array.Where(x => x.TryCast<Vent>() != null).Select(x => x.TryCast<Vent>()!))
                {
                    bool flag4;
                    float num4 = usable2.CanUse(data, forVenting, distance, out flag4);
                    if (flag4 && num4 < num3)
                    {
                        list.Add(usable2);
                    }
                }
            }
        }

        var vent = (list.Count > 0) ? list.FirstOrDefault() : null;

        return vent;
    }


    public static Vent? GetClosestUsableVent(bool forVenting)
    {
        var playerControl = PlayerControl.LocalPlayer;
        var data = playerControl.Data;
        Vector2 truePosition = playerControl.GetTruePosition();
        var flag2 = (playerControl.CanMove || playerControl.inVent || !forVenting);
        int num2 = Physics2D.OverlapCircleNonAlloc(truePosition, playerControl.MaxReportDistance,
            playerControl.hitBuffer, Constants.Usables);
        float num3 = float.MaxValue;
        List<Vent> list = new List<Vent>();
        for (int i = 0; i < num2; i++)
        {
            Collider2D collider2D = playerControl.hitBuffer[i];
            if (!playerControl.cache.TryGetValue(collider2D, out var array))
            {
                continue;
            }

            if (flag2)
            {
                foreach (var usable2 in array.Where(x => x.TryCast<Vent>() != null).Select(x => x.TryCast<Vent>()!))
                {
                    bool flag4;
                    float num4 = usable2.CanUse(data, forVenting, out flag4);
                    if (flag4 && num4 < num3)
                    {
                        list.Add(usable2);
                    }
                }
            }
        }

        var vent = (list.Count > 0) ? list.FirstOrDefault() : null;

        return vent;
    }

    public static float CanUse(this Vent vent, NetworkedPlayerInfo pc, bool toVent, float distance, out bool couldUse)
    {
        float num = float.MaxValue;
        PlayerControl @object = pc.Object;
        couldUse = !toVent || !@object.MustCleanVent(vent.Id) || Vent.currentVent == vent;

        var @event = new PlayerCanUseEvent(vent.Cast<IUsable>());
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            return num;
        }

        if (VanillaSystemCheckPatches.VentSystem != null &&
            VanillaSystemCheckPatches.VentSystem.IsVentCurrentlyBeingCleaned(vent.Id))
        {
            couldUse = false;
        }

        if (couldUse)
        {
            Vector3 center = @object.Collider.bounds.center;
            Vector3 position = vent.transform.position;
            num = Vector2.Distance(center, position);
            couldUse &= (num <= distance &&
                         !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask,
                             false));
        }

        return num;
    }

    public static float CanUse(this Vent vent, NetworkedPlayerInfo pc, bool toVent, out bool couldUse)
    {
        float num = float.MaxValue;
        PlayerControl @object = pc.Object;
        couldUse = !toVent || !@object.MustCleanVent(vent.Id) || Vent.currentVent == vent;

        var @event = new PlayerCanUseEvent(vent.Cast<IUsable>());
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            couldUse = false;
            return num;
        }

        if (VanillaSystemCheckPatches.VentSystem != null &&
            VanillaSystemCheckPatches.VentSystem.IsVentCurrentlyBeingCleaned(vent.Id))
        {
            couldUse = false;
        }

        if (couldUse)
        {
            Vector3 center = @object.Collider.bounds.center;
            Vector3 position = vent.transform.position;
            num = Vector2.Distance(center, position);
            couldUse &= (num <= vent.UsableDistance &&
                         !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask,
                             false));
        }

        return num;
    }
}
