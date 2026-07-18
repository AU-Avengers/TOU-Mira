using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Modifiers;

public static class NoisemakerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.IsRole<SoulCollectorRole>() || MeetingHud.Instance)
        {
            return;
        }

        if (@event.Target.HasModifier<NoisemakerModifier>())
        {
            NoisemakerModifier.NotifyOfDeath(@event.Target, false);
        }
        else if (@event.Target.GetRoleWhenAlive().Role is RoleTypes.Noisemaker)
        {
            NoisemakerModifier.NotifyOfDeath(@event.Target, true);
        }
    }
}