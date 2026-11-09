using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class HypnotistEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        foreach (var mod in ModifierUtils.GetActiveModifiers<HypnotisedModifier>())
        {
            mod.UnHysteria();
        }
    }

    [RegisterEvent(1000)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        foreach (var mod in ModifierUtils.GetActiveModifiers<HypnotisedModifier>())
        {
            if (mod.Hypnotist == exiled || mod.Hypnotist == null || mod.Hypnotist.HasDied() ||
                mod.Hypnotist.Data.Disconnected || mod.Hypnotist.Data.Role is not HypnotistRole)
            {
                mod.RemoveSelf();
            }
        }

        foreach (var hypnotist in CustomRoleUtils.GetActiveRolesOfType<HypnotistRole>())
        {
            if (hypnotist.Player == exiled || hypnotist.Player == null || hypnotist.Player.HasDied() ||
                hypnotist.Player.Data.Disconnected)
            {
                hypnotist.HysteriaActive = false;
            }
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        foreach (var hypnotist in CustomRoleUtils.GetActiveRolesOfType<HypnotistRole>())
        {
            if (!hypnotist.HysteriaActive || hypnotist.Player == null || hypnotist.Player.HasDied() ||
                hypnotist.Player.Data.Disconnected)
            {
                continue;
            }

            var mods = ModifierUtils.GetActiveModifiers<HypnotisedModifier>(x => x.Hypnotist == hypnotist.Player);
            mods.Do(x => x.Hysteria());
        }
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player.Data.Role is HypnotistRole hypno)
        {
            hypno.HysteriaActive = false;
            ModifierUtils.GetPlayersWithModifier<HypnotisedModifier>(x => x.Hypnotist == hypno.Player)
                .Do(x => x.RemoveModifier<HypnotisedModifier>());
        }
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        var player = @event?.ClientData?.Character;
        if (player == null || player.Data == null)
        {
            return;
        }

        if (player.Data.Role is HypnotistRole hypno)
        {
            hypno.HysteriaActive = false;
            ModifierUtils.GetPlayersWithModifier<HypnotisedModifier>(x => x.Hypnotist == player)
                .Do(x => x.RemoveModifier<HypnotisedModifier>());
        }
    }
}
