using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Networking;

internal readonly record struct PuppeteerInputPacket(byte ControlledId, Vector2 Direction, Vector2 Position, bool InAnim, Vector2 Velocity);

[RegisterCustomRpc((uint)TownOfUsRpc.PuppeteerInputUnreliable)]
internal sealed class PuppeteerInputUnreliableRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, PuppeteerInputPacket>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
    public override SendOption SendOption => (SendOption)1;

    public override void Write(MessageWriter writer, PuppeteerInputPacket data)
    {
        writer.Write(data.ControlledId);
        writer.Write(data.Direction);
        writer.Write(data.Position);
        writer.Write(data.InAnim);
        writer.Write(data.Velocity);
    }

    public override PuppeteerInputPacket Read(MessageReader reader)
    {
        var controlledId = reader.ReadByte();
        var dir = reader.ReadVector2();
        var pos = reader.ReadVector2();
        var ani = reader.ReadBoolean();
        var vel = reader.ReadVector2();
        return new PuppeteerInputPacket(controlledId, dir, pos, ani, vel);
    }

    public override void Handle(PlayerControl sender, PuppeteerInputPacket data)
    {
        var controlledPlayerInfo = GameData.Instance?.GetPlayerById(data.ControlledId);
        var controlled = controlledPlayerInfo?.Object;
        if (controlled == null)
        {
            return;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return;
        }

        if (sender == null ||
            !PuppeteerRole.ControlState.IsControlled(data.ControlledId, out var controllerId) ||
            controllerId != sender.PlayerId)
        {
            return;
        }

        PuppeteerRole.ControlState.SetForcedDirection(data.ControlledId, data.Direction);
        PuppeteerRole.ControlState.SetMovementState(data.ControlledId, data.Position, data.InAnim, data.Velocity);
    }
}