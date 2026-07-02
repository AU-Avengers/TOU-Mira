using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class MysticEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        var victim = @event.Target;

        if (PlayerControl.LocalPlayer.Data.Role is MysticRole && !victim.AmOwner)
        {
            victim.AddModifier<MysticDeathNotifierModifier>(PlayerControl.LocalPlayer);
        }
    }
}