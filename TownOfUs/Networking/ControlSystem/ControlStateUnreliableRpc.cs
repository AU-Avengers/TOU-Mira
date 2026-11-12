using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using UnityEngine;

namespace TownOfUs.Networking.ControlSystem;

internal readonly record struct ControlStatePacket(byte TargetId, Vector2 Position, bool InAnim, Vector2 Velocity)
    : IMovementStateDataPacket;

[RegisterCustomRpc((uint)TownOfUsRpc.ControlStateUnreliable)]
internal sealed class ControlStateUnreliableRpc(TownOfUsPlugin plugin, uint id)
    : ControlDataUnreliableRpc<TownOfUsPlugin, ControlStatePacket>(plugin, id)
{
    public override void Write(MessageWriter writer, ControlStatePacket data)
    {
        writer.Write(data.TargetId);
        writer.Write(data.Position);
        writer.Write(data.InAnim);
        writer.Write(data.Velocity);
    }

    public override ControlStatePacket Read(MessageReader reader)
    {
        var controlledId = reader.ReadByte();
        var pos = reader.ReadVector2();
        var ani = reader.ReadBoolean();
        var vel = reader.ReadVector2();
        return new ControlStatePacket(controlledId, pos, ani, vel);
    }
}
