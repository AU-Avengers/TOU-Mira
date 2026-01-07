using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class MinerEvents
{
    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (OptionGroupSingleton<MinerOptions>.Instance.MineVisibility == MineVisiblityOptions.Immediate)
        {
            return;
        }

        if (!@event.IsVent)
        {
            return;
        }

        var vent = @event.Usable.TryCast<Vent>();

        if (vent == null)
        {
            return;
        }

        if (vent.name.Contains("MinerVent") && PlayerControl.LocalPlayer.Data.Role is not MinerRole &&
            !vent.myRend.enabled)
        {
            @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void EnterVentEventHandler(EnterVentEvent @event)
    {
        if (OptionGroupSingleton<MinerOptions>.Instance.MineVisibility == MineVisiblityOptions.Immediate)
        {
            return;
        }

        var player = @event.Player;
        var vent = @event.Vent;

        if (player.Data.Role is not MinerRole)
        {
            return;
        }

        if (vent == null || !vent.name.Contains($"MinerVent-{player.PlayerId}"))
        {
            return;
        }

        MinerRole.RpcShowVent(player, vent.Id);
    }

    [RegisterEvent]
    public static void ExitVentEventHandler(ExitVentEvent @event)
    {
        if (OptionGroupSingleton<MinerOptions>.Instance.MineVisibility == MineVisiblityOptions.Immediate)
        {
            return;
        }

        var player = @event.Player;
        var vent = @event.Vent;

        if (player.Data.Role is not MinerRole)
        {
            return;
        }

        if (vent == null || !vent.name.Contains($"MinerVent-{player.PlayerId}"))
        {
            return;
        }

        MinerRole.RpcShowVent(player, vent.Id);
    }
}