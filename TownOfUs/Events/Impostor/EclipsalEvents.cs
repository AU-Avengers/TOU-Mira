using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;

namespace TownOfUs.Events.Impostor;

public static class EclipsalEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        // var source = @event.Source;
        var victim = @event.Target;
        if (!victim.TryGetModifier<EclipsalBlindModifier>(out var mod))
        {
            return;
        }

        /*
        if (source.AmOwner && source.Data.Role is EclipsalRole && source != victim)
        {
            var ach = AchievementsTabSingleton<TouImpRoleAchievementsTab>.Instance.EternalDarkness;
            if (!ach.Unlocked)
            {
                ach.Increment(1, ach.CurrentValue == 3);
            }
        }*/

        victim.RemoveModifier(mod);
    }
}