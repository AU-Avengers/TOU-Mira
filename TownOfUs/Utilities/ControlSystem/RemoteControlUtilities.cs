using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TownOfUs.Interfaces;
using TownOfUs.Modules.ControlSystem;
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

    [MethodRpc((uint)TownOfUsRpc.StartRemoteControl)]
    public static void RpcStartRemoteControl(PlayerControl controller, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(controller);
            return;
        }

        if (!controller.TryGetRemoteControl<IControlStarter>(out var starter))
        {
            Error("RpcStartControl - Invalid controller");
            return;
        }

        if (target?.Data == null || target.HasDied())
        {
            return;
        }

        if (!starter.IsValidTarget(target))
        {
            return;
        }

        Coroutines.Start(starter.CoFinishControl(target));
    }

    public static IEnumerator CoFinishControl(this IControlStarter starter, PlayerControl target)
    {
        var controller = starter.Player;
        while (target.IsInTargetingAnimState())
        {
            yield return null;
        }

        IController control = starter.SetupController(target);

        starter.State.SetControl(target.PlayerId, controller.PlayerId);

        starter.SetupTarget(target);

        if (target.inVent)
        {
            control.ClearTargetInVent(false);
        }

        SnapControlTarget(target, controller);

        if (starter.State is DualControlState { ApplyToController: true })
        {
            controller.NetTransform?.SnapTo(target.transform.position);
        }

        if (controller.AmOwner)
        {
            starter.CreateNotification();
            control.SetupLocal(false);
        }
        else if (target.AmOwner)
        {
            control.SetupTargetLocal(false);
        }
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
                control.ClearTargetInVent(true);
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
            control.SetupLocal(true);
        }
        else if (target.AmOwner)
        {
            control.SetupTargetLocal(true);
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
