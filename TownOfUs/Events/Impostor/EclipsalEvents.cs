using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Achievements;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class EclipsalEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        if (!victim.TryGetModifier<EclipsalBlindModifier>(out var mod))
        {
            return;
        }

        if (source.AmOwner && source.Data.Role is EclipsalRole && source != victim)
        {
            var ach = AchievementsTabSingleton<TouImpRoleAchievementsTab>.Instance.EternalDarkness;
            ach.Increment(1, ach.CurrentValue == 2);
        }

        victim.RemoveModifier(mod);
    }
}