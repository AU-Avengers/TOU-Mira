using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.HnsCrewmate;
using TownOfUs.Roles.HnsCrewmate;

namespace TownOfUs.Events.HnsCrewmate;

public static class HiderMysticEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var victim = @event.Target;

        if (PlayerControl.LocalPlayer.Data.Role is HnsMysticRole)
        {
            victim?.AddModifier<HnsMysticDeathNotifModifier>(PlayerControl.LocalPlayer);
        }
    }
}