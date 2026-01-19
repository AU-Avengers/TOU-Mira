using Hazel;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace TownOfUs.Networking;

internal enum MultiplayerFreeplayAction : byte
{
    SetRole = 1,
    ToggleModifier = 2,
    SetLovers = 3,
    RemoteKill = 4,
    Reset = 5,
}

internal readonly struct MultiplayerFreeplayRequest
{
    public MultiplayerFreeplayRequest(MultiplayerFreeplayAction action, byte targetId, byte otherId, ushort data)
    {
        Action = action;
        TargetId = targetId;
        OtherId = otherId;
        Data = data;
    }

    public MultiplayerFreeplayAction Action { get; }
    public byte TargetId { get; }
    public byte OtherId { get; }
    public ushort Data { get; }
}

[RegisterCustomRpc((uint)TownOfUsRpc.MultiplayerFreeplayRequest)]
internal sealed class MultiplayerFreeplayRequestRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, MultiplayerFreeplayRequest>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Write(MessageWriter writer, MultiplayerFreeplayRequest data)
    {
        writer.Write((byte)data.Action);
        writer.Write(data.TargetId);
        writer.Write(data.OtherId);
        writer.Write(data.Data);
    }

    public override MultiplayerFreeplayRequest Read(MessageReader reader)
    {
        var action = (MultiplayerFreeplayAction)reader.ReadByte();
        var targetId = reader.ReadByte();
        var otherId = reader.ReadByte();
        var data = reader.ReadUInt16();
        return new MultiplayerFreeplayRequest(action, targetId, otherId, data);
    }

    public override void Handle(PlayerControl sender, MultiplayerFreeplayRequest data)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (TutorialManager.InstanceExists ||
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
            !OptionGroupSingleton<HostSpecificOptions>.Instance.MultiplayerFreeplay.Value)
        {
            return;
        }

        if (sender == null || sender.Data == null || sender.Data.Disconnected)
        {
            return;
        }

        switch (data.Action)
        {
            case MultiplayerFreeplayAction.SetRole:
                HandleSetRole(data.TargetId, data.Data);
                break;

            case MultiplayerFreeplayAction.ToggleModifier:
                HandleToggleModifier(data.TargetId, data.Data);
                break;

            case MultiplayerFreeplayAction.SetLovers:
                HandleSetLovers(data.TargetId, data.OtherId);
                break;

            case MultiplayerFreeplayAction.RemoteKill:
                HandleRemoteKill(data.TargetId, data.OtherId);
                break;

            case MultiplayerFreeplayAction.Reset:
                MultiplayerFreeplayDebugState.RestoreBaseline();
                break;
        }
    }

    private static PlayerControl? GetPlayer(byte playerId)
    {
        var info = GameData.Instance?.GetPlayerById(playerId);
        var plr = info?.Object;
        if (plr == null || plr.Data == null || plr.Data.Disconnected)
        {
            return null;
        }

        return plr;
    }

    private static void EnsureAlive(PlayerControl plr)
    {
        if (!plr.HasDied())
        {
            return;
        }

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == plr.PlayerId);
        if (body != null)
        {
            try { UnityEngine.Object.Destroy(body.gameObject); } catch { /* ignored */ }
        }

        GameHistory.ClearMurder(plr);
        plr.Revive();
        DeathStateSync.RpcSyncDeathState(plr, false);
    }

    private static void HandleSetRole(byte targetId, ushort roleType)
    {
        var target = GetPlayer(targetId);
        if (target == null)
        {
            return;
        }

        EnsureAlive(target);
        target.RpcChangeRole(roleType);
    }

    private static void HandleToggleModifier(byte targetId, ushort modifierId)
    {
        var target = GetPlayer(targetId);
        if (target == null)
        {
            return;
        }

        if (!MultiplayerFreeplayRegistry.TryGetModifierType(modifierId, out var modifierType) || modifierType == null)
        {
            return;
        }

        // Lovers requires selecting two players; it's handled as a separate action.
        if (modifierType == typeof(LoverModifier))
        {
            return;
        }

        var comp = target.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        var existing = target.GetModifiers<BaseModifier>().Where(x => x.GetType() == modifierType).ToList();
        if (existing.Count > 0)
        {
            foreach (var mod in existing)
            {
                comp.RemoveModifier(mod);
            }

            return;
        }

        if (!MultiplayerFreeplayRegistry.IsModifierAddableWithoutParameters(modifierType))
        {
            return;
        }

        if (Activator.CreateInstance(modifierType) is BaseModifier instance)
        {
            comp.AddModifier(instance);
        }
    }

    private static void HandleSetLovers(byte loverAId, byte loverBId)
    {
        var loverA = GetPlayer(loverAId);
        var loverB = GetPlayer(loverBId);
        if (loverA == null || loverB == null || loverA == loverB)
        {
            return;
        }

        EnsureAlive(loverA);
        EnsureAlive(loverB);
        LoverModifier.DebugSetLovers(loverA, loverB, clearExisting: true);
    }

    private static void HandleRemoteKill(byte killerId, byte victimId)
    {
        var killer = GetPlayer(killerId);
        var victim = GetPlayer(victimId);
        if (killer == null || victim == null)
        {
            return;
        }

        if (victim.HasDied())
        {
            return;
        }

        killer.RpcCustomMurder(victim);
    }
}