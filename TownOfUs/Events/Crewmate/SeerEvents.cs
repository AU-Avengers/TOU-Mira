using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

[RegisterEventContainer]
public static class SeerEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        foreach (var seer in CustomRoleUtils.GetActiveRolesOfType<SeerRole>())
        {
            seer.UsedThisRound = false;
        }
    }
}
