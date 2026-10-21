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
        if (!victim.TryRemoveModifier<EclipsalBlindModifier>())
        {
            return;
        }

        if (source.AmOwner && source.Data.Role is EclipsalRole && source != victim)
        {
            AchievementsAPIHelper.Increment(TouAchievements.EternalDarkness, 1,
                AchievementsAPIHelper.GetCurrentValue(TouAchievements.EternalDarkness) == 2);
        }
    }
}