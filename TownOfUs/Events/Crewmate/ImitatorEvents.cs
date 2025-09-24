using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class ImitatorEvents
{
    [RegisterEvent(1001)]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }
        var imitatorRoles = CustomRoleUtils.GetActiveRolesOfType<ImitatorRole>();

        if (imitatorRoles.Any())
        {
            foreach (var imitatorPlayer in imitatorRoles)
            {
                if (imitatorPlayer.Player.HasModifier<ImitatorCacheModifier>())
                {
                    continue;
                }
                imitatorPlayer.Player.AddModifier<ImitatorCacheModifier>();
            }
        }
        
        var imitators = ModifierUtils.GetActiveModifiers<ImitatorCacheModifier>();

        if (!imitators.Any())
        {
            return;
        }

        foreach (var mod in imitators)
        {
            if (mod.Player.AmOwner)
            {
                mod.UpdateRole();
            }
        }
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var player = @event.Player;

        if (player.HasModifier<ImitatorCacheModifier>() && !@event.NewRole.IsCrewmate())
        {
            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Error($"Removed Imitator Cache Modifier On Role Change");
            player.RemoveModifier<ImitatorCacheModifier>();
        }
    }
}