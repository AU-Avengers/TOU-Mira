using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class MediumEvents
{
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent _)
    {
        if (PlayerControl.LocalPlayer.Data.Role is MediumRole medium)
        {
            medium.MediatedPlayers.ForEach(mediated => mediated.Player?.RpcRemoveModifier(mediated.UniqueId));
        }
    }
}