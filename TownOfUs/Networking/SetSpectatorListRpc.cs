using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Patches.Misc;

namespace TownOfUs.Networking;

[RegisterCustomRpc((uint)TownOfUsRpc.SetSpectatorList)]
public sealed class SetSpectatorListRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, List<string>>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Write(MessageWriter writer, List<string>? data)
    {
        if (data == null)
        {
            writer.Write((byte)0);
            return;
        }

        writer.Write((byte)data.Count);
        foreach (var kvp in data)
        {
            writer.Write(kvp);
        }
    }

    public override List<string> Read(MessageReader reader)
    {
        var count = reader.ReadByte();
        var data = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var key = reader.ReadByte();
            var value = reader.ReadString();
            data[key] = value;
        }

        return data;
    }

    public override void Handle(PlayerControl innerNetObject, List<string>? data)
    {
        if (data == null || data.Count == 0)
        {
            return;
        }

        ChatPatches.SetSpectatorList(data);
    }
}