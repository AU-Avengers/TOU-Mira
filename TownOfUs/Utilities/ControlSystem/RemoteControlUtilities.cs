using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using System.Diagnostics.CodeAnalysis;
using TownOfUs.Interfaces;
using UnityEngine;

namespace TownOfUs.Utilities.ControlSystem;

public static class RemoteControlUtilities
{
    public static TControl? GetRemoteControl<TControl>(this PlayerControl player, Func<TControl, bool> predicate = null!)
        where TControl : class, IRemoteControl
    {
        if (player.Data?.Role is TControl role && (predicate == null || predicate(role)))
        {
            return role;
        }
        if (player.TryGetModifierOfType(out var mod, predicate))
        {
            return mod;
        }
        return null;
    }

    public static bool TryGetRemoteControl<TControl>(this PlayerControl player, [NotNullWhen(true)] out TControl? control, Func<TControl, bool> predicate = null!)
        where TControl : class, IRemoteControl
    {
        control = player.GetRemoteControl(predicate);
        return control != null;
    }

    public static void EnsureControlRole(Type type)
    {
        if (!typeof(ITownOfUsControlRole).IsAssignableFrom(type) || !type.IsClass)
        {
            throw new ArgumentException("Type must be class and implement " + nameof(ITownOfUsControlRole), nameof(type));
        }
    }

    public static bool ControllerDeadCheck(this IControlledModifier controlled)
    {
        if (controlled.ControllerIsDead is not { } isDead)
        {
            return true;
        }

        return controlled.Controller.HasDied() == isDead;
    }

    [MethodRpc((uint)TownOfUsRpc.ControllerEndControl)]
    public static void RpcControllerEndControl(PlayerControl controller, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(controller);
            return;
        }

        ControllerEndControl(controller, target);
    }
    
    public static void ControllerEndControl(PlayerControl controller, PlayerControl target)
    {
        if (controller == null || target == null)
        {
            return;
        }

        if (controller.GetRemoteControl<IController>() is not { } control || control.Controlled != target)
        {
            return;
        }

        control.State.ClearControl(target.PlayerId);
        target.GetModifierOfType<IControlledModifier>()?.RemoveSelf();

        if (target.MyPhysics != null)
        {
            if (target.inVent)
            {
                control.ClearTargetInVent();
            }
            else
            {
                target.MyPhysics.body?.velocity = Vector2.zero;
                target.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            }
        }

        NetTransformBacklogUtils.FlushBacklog(target);

        SnapControlTarget(target, controller);

        control.ClearOnEnd();

        SnapControlTarget(controller);

        if (controller.AmOwner)
        {
            control.SetupLocalOnClear(controller);
        }

        control.ClearNotification();

        control.PostEndClear();
    }

    public static void SnapControlTarget(PlayerControl target, PlayerControl? other = null)
    {
        if (target.AmOwner)
        {
            var pos = (Vector2)target.transform.position;
            try
            {
                target.NetTransform?.SnapTo(pos);
            }
            catch
            {
                // ignored
            }
        }
        else if (other != null && other.AmOwner)
        {
            NetTransformBacklogUtils.FlushAndSnap(target);
        }
        else
        {
            NetTransformBacklogUtils.FlushBacklog(target);
        }
    }
}
