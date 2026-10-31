using BepInEx.Unity.IL2CPP;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using UnityEngine;

namespace TownOfUs.Networking;

public interface ITargetDataPacket
{
    byte TargetId { get; }
}

public interface IMovementStateDataPacket : ITargetDataPacket
{
    Vector2 Position { get; }
    bool InAnim { get; }
    Vector2 Velocity { get; }
}

public interface IDirectionDataPacket : ITargetDataPacket
{
    Vector2 Direction { get; }
}

public interface IBidirectionalDataPacket : IDirectionDataPacket
{
    bool FromVictim { get; }
}

public abstract class ControlDataUnreliableRpc<TPlugin, TData>(TPlugin plugin, uint id)
    : PlayerCustomRpc<TPlugin, TData>(plugin, id)
    where TPlugin : BasePlugin
    where TData : ITargetDataPacket
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Handle(PlayerControl sender, TData? data)
    {
        if (data == null!)
        {
            return;
        }

        var targetPlayerInfo = GameData.Instance?.GetPlayerById(data.TargetId);
        var target = targetPlayerInfo?.Object;
        if (target == null || sender == null)
        {
            return;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return;
        }

        if (!ControlState.IsUsingAnyState(data.TargetId, out var otherId, out var isControlled, out var controlState) ||
            otherId != sender.PlayerId)
        {
            return;
        }

        if (data is IMovementStateDataPacket moveState && isControlled)
        {
            controlState.SetMovementState(data.TargetId, moveState.Position, moveState.InAnim, moveState.Velocity);
        }

        if (data is IDirectionDataPacket dirData)
        {
            if (dirData is IBidirectionalDataPacket { FromVictim: true } && !isControlled)
            {
                controlState.SetSelfDirection(data.TargetId, dirData.Direction);
            }
            else if (isControlled)
            {
                controlState.SetForcedDirection(data.TargetId, dirData.Direction);
            }
        }
    }
}
