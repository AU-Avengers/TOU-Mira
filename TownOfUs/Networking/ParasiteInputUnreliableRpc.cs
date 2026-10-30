using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using UnityEngine;

namespace TownOfUs.Networking;

internal readonly record struct ParasiteInputPacket(byte TargetId, Vector2 Direction, Vector2 Position, bool InAnim, Vector2 Velocity)
    : IMovementStateDataPacket, IDirectionDataPacket;

[RegisterCustomRpc((uint)TownOfUsRpc.ParasiteInputUnreliable)]
internal sealed class ParasiteInputUnreliableRpc(TownOfUsPlugin plugin, uint id)
    : ControlDataUnreliableRpc<TownOfUsPlugin, ParasiteInputPacket>(plugin, id)
{
    public override void Write(MessageWriter writer, ParasiteInputPacket data)
    {
        writer.Write(data.TargetId);
        writer.Write(data.Direction);
        writer.Write(data.Position);
        writer.Write(data.InAnim);
        writer.Write(data.Velocity);
    }

    public override ParasiteInputPacket Read(MessageReader reader)
    {
        var controlledId = reader.ReadByte();
        var dir = reader.ReadVector2().ApplyDeadzone();
        var pos = reader.ReadVector2();
        var ani = reader.ReadBoolean();
        var vel = reader.ReadVector2();
        return new ParasiteInputPacket(controlledId, dir, pos, ani, vel);
    }
}