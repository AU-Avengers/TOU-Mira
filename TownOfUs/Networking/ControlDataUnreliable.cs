using BepInEx.Unity.IL2CPP;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;

namespace TownOfUs.Networking;

public interface ITargetDataPacket
{
    byte TargetId { get; }
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

        Store(sender, data);
    }

    protected abstract void Store(PlayerControl sender, TData data);
}
