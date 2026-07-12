using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class EscapistEvents
{
    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent _)
    {
        CustomRoleUtils.GetActiveRolesOfType<EscapistRole>().Do(x => x.MarkedLocation = null);
    }
}