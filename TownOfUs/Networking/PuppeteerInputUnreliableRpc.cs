using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using UnityEngine;

namespace TownOfUs.Networking;

internal readonly struct PuppeteerInputPacket
{
    public PuppeteerInputPacket(byte controlledId, Vector2 direction)
    {
        ControlledId = controlledId;
        Direction = direction;
    }

    public byte ControlledId { get; }
    public Vector2 Direction { get; }
}

[RegisterCustomRpc((uint)TownOfUsInternalRpc.PuppeteerInputUnreliable)]
internal sealed class PuppeteerInputUnreliableRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, PuppeteerInputPacket>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
    public override SendOption SendOption => (SendOption)1;

    public override void Write(MessageWriter writer, PuppeteerInputPacket data)
    {
        writer.Write(data.ControlledId);
        writer.Write(data.Direction);
    }

    public override PuppeteerInputPacket Read(MessageReader reader)
    {
        var controlledId = reader.ReadByte();
        var dir = reader.ReadVector2();
        return new PuppeteerInputPacket(controlledId, dir);
    }

    public override void Handle(PlayerControl sender, PuppeteerInputPacket data)
    {
        var controlledPlayerInfo = GameData.Instance?.GetPlayerById(data.ControlledId);
        var controlled = controlledPlayerInfo?.Object;
        if (controlled == null || !controlled.AmOwner)
        {
            return;
        }

        // Ignore input packets during Time Lord rewind - rewind handles movement
        if (TimeLordRewindSystem.IsRewinding)
        {
            return;
        }

        // If control ended (meeting called / puppeteer stopped controlling), ignore any late unreliable packets.
        if (sender == null ||
            !PuppeteerControlState.IsControlled(data.ControlledId, out var controllerId) ||
            controllerId != sender.PlayerId)
        {
            return;
        }

        PuppeteerControlState.SetDirection(data.ControlledId, data.Direction);
    }
}


