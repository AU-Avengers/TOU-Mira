using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Networking;

internal readonly struct ParasiteMovePacket
{
    public ParasiteMovePacket(byte controlledId, Vector2 position, Vector2 velocity)
    {
        ControlledId = controlledId;
        Position = position;
        Velocity = velocity;
    }

    public byte ControlledId { get; }
    public Vector2 Position { get; }
    public Vector2 Velocity { get; }
}

[RegisterCustomRpc((uint)TownOfUsInternalRpc.ParasiteMoveUnreliable)]
internal sealed class ParasiteMoveUnreliableRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, ParasiteMovePacket>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
    public override SendOption SendOption => (SendOption)1;

    public override void Write(MessageWriter writer, ParasiteMovePacket data)
    {
        writer.Write(data.ControlledId);
        writer.Write(data.Position);
        writer.Write(data.Velocity);
    }

    public override ParasiteMovePacket Read(MessageReader reader)
    {
        var controlledId = reader.ReadByte();
        var pos = reader.ReadVector2();
        var vel = reader.ReadVector2();
        return new ParasiteMovePacket(controlledId, pos, vel);
    }

    public override void Handle(PlayerControl sender, ParasiteMovePacket data)
    {
        var controlledPlayerInfo = GameData.Instance?.GetPlayerById(data.ControlledId);
        var controlled = controlledPlayerInfo?.Object;
        if (controlled == null || !controlled.AmOwner)
        {
            return;
        }

        // Ignore movement packets during Time Lord rewind - rewind handles movement
        if (TimeLordRewindSystem.IsRewinding)
        {
            return;
        }

        // If control ended (meeting called / parasite stopped controlling), ignore any late unreliable packets.
        // These can otherwise snap the victim back to the "end of parasite" position after a meeting.
        if (sender == null ||
            !ParasiteControlState.IsControlled(data.ControlledId, out var controllerId) ||
            controllerId != sender.PlayerId)
        {
            return;
        }

        var body = controlled.MyPhysics?.body;
        var currentPos = body != null ? body.position : (Vector2)controlled.transform.position;
        var currentVel = body != null ? body.velocity : Vector2.zero;

        var targetPos = data.Position;
        var targetVel = data.Velocity;

        var dist = Vector2.Distance(currentPos, targetPos);

        var isMoving = targetVel.sqrMagnitude > 0.001f;

        float posAlpha = 0.55f;

        if (isMoving)
            posAlpha = 0.85f;

        if (dist > 0.6f)
            posAlpha = 1f;

        var velAlpha = isMoving ? 0.25f : 0.65f;

        var smoothedPos = Vector2.Lerp(currentPos, targetPos, posAlpha);
        var smoothedVel = Vector2.Lerp(currentVel, targetVel, velAlpha);

        ParasiteControlState.SetMovementState(data.ControlledId, smoothedPos, smoothedVel);

        controlled.transform.position = smoothedPos;
        if (body != null)
        {
            body.position = smoothedPos;
            body.velocity = smoothedVel;
        }
    }
}