using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Impostor;

public static class HypnotistEvents
{
    [RegisterEvent(1000)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        foreach (var mod in ModifierUtils.GetActiveModifiers<HypnotisedModifier>())
        {
            if (mod.Hypnotist == exiled || mod.Hypnotist == null || mod.Hypnotist.HasDied() || mod.Hypnotist.Data.Role is not HypnotistRole)
            {
                mod.ModifierComponent?.RemoveModifier(mod);
            }
        }
        
        foreach (var hypnotist in CustomRoleUtils.GetActiveRolesOfType<HypnotistRole>())
        {
            if (!hypnotist.HysteriaActive)
            {
                continue;
            }

            var mods = ModifierUtils.GetActiveModifiers<HypnotisedModifier>(x => x.Hypnotist == hypnotist.Player);
            mods.Do(x => x.Hysteria());
        }
    }

    /* whenever the event gets changed to having the character not being null
    [RegisterEvent]
    public static void HypnotistDisconnectHandler(PlayerLeaveEvent @event)
    {
        if (@event.ClientData.Character.Data.Role is HypnotistRole hypno)
        {
            ModifierUtils.GetPlayersWithModifier<HypnotisedModifier>(x => x.Hypnotist == hypno)
                         .Do(x => x.RemoveModifier<HypnotisedModifier>());
        }
    }*/
}