using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using UnityEngine;

namespace TownOfUs.Networking.ControlSystem;

internal readonly record struct ControlDirectionPacket(byte TargetId, bool FromVictim, Vector2 Direction)
    : IBidirectionalDataPacket;

[RegisterCustomRpc((uint)TownOfUsRpc.ControlDirectionUnreliable)]
internal sealed class ControlDirectionUnreliableRpc(TownOfUsPlugin plugin, uint id)
    : ControlDataUnreliableRpc<TownOfUsPlugin, ControlDirectionPacket>(plugin, id)
{
    public override void Write(MessageWriter writer, ControlDirectionPacket data)
    {
        writer.Write(data.TargetId);
        writer.Write(data.FromVictim);
        writer.Write(data.Direction);
    }

    public override ControlDirectionPacket Read(MessageReader reader)
    {
        var controlledId = reader.ReadByte();
        var fromV = reader.ReadBoolean();
        var dir = reader.ReadVector2().ApplyDeadzone();
        return new ControlDirectionPacket(controlledId, fromV, dir);
    }
}
