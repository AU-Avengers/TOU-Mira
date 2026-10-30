using BepInEx.Unity.IL2CPP;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;

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

        if (!ControlState.IsUsingAnyState(data.TargetId, out var otherId, out var isControlled, out _) ||
            otherId != sender.PlayerId)
        {
            return;
        }

        Store(isControlled, data);
    }

    protected abstract void Store(bool isControlled, TData data);
}
