using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
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
                // TODO: vent cleanup
            }
            else
            {
                target.MyPhysics.body?.velocity = Vector2.zero;
                target.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            }
        }

        if (target.NetTransform != null)
        {
            try
            {
                var finalPos = (Vector2)target.transform.position;

                NetTransformBacklogUtils.FlushBacklog(target);

                if (target.AmOwner)
                {
                    target.NetTransform.SnapTo(finalPos);
                }
                else if (controller.AmOwner)
                {
                    NetTransformBacklogUtils.FlushAndSnap(target);
                }
                else
                {
                    NetTransformBacklogUtils.FlushBacklog(target);
                }
            }
            catch
            {
                // ignored
            }
        }

        control.ClearOnEnd();

        if (controller.AmOwner)
        {
            var pos = (Vector2)controller.transform.position;
            if (controller.NetTransform != null)
            {
                try
                {
                    controller.NetTransform.SnapTo(pos);
                }
                catch
                {
                    // ignored
                }
            }
        }
        else
        {
            NetTransformBacklogUtils.FlushBacklog(controller);
        }

        if (controller.AmOwner)
        {
            control.SetupLocalOnClear(controller);
        }

        control.ClearNotification();

        // TODO: Post setup
    }
}
