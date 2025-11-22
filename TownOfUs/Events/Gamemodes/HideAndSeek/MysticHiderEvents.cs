using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Gamemodes.HideAndSeek.Crewmate;
using TownOfUs.Roles.Gamemodes.CrewmateHiders;

namespace TownOfUs.Events.Gamemodes.HideAndSeek;

public static class HiderMysticEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var victim = @event.Target;

        if (PlayerControl.LocalPlayer.Data.Role is MysticHiderRole)
        {
            victim?.AddModifier<MysticHiderDeathNotifierModifier>(PlayerControl.LocalPlayer);
        }
    }
}