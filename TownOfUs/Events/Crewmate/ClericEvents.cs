using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class ClericEvents
{
    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent @event)
    {
        ModifierUtils.GetPlayersWithModifier<ClericCleanseModifier>()
            .Do(x => x.RemoveModifier<ClericCleanseModifier>());
    }

    [RegisterEvent(-800)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        CheckForClericBarrier(@event, target, PlayerControl.LocalPlayer);
    }

    [RegisterEvent(-800)]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target && !target!.HasModifier<ClericBarrierModifier>())
        {
            return;
        }

        ResetButtonTimer(source, button);
    }

    [RegisterEvent(-800)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForClericBarrier(@event, target, source))
        {
            ResetButtonTimer(source);
        }
    }

    private static bool CheckForClericBarrier(MiraCancelableEvent @event, PlayerControl target,
        PlayerControl source)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (!target.HasModifier<ClericBarrierModifier>() ||
            target.PlayerId == source.PlayerId ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, $"{target.Data.PlayerName} has a cleric barrier, stopping an interaction from {source.Data.PlayerName}!");
        @event.Cancel();

        var cleric = target.GetModifier<ClericBarrierModifier>()?.Cleric.GetRole<ClericRole>();

        if (cleric != null && (TutorialManager.InstanceExists || source.AmOwner))
        {
            ClericRole.RpcClericBarrierAttacked(source, cleric.Player, target);
        }

        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (!source.AmOwner)
        {
            return;
        }

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);
        source.SetKillTimer(reset);
    }
}