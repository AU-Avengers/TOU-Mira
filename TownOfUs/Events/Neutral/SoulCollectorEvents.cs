using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class SoulCollectorEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!CustomRoleUtils.GetActiveRolesOfType<SoulCollectorRole>().Any())
        {
            return;
        }

        var body = @event.DeadBody;
        if (body == null)
        {
            return;
        }

        body.gameObject.AddComponent<SoulCollectibleBody>();
    }
}