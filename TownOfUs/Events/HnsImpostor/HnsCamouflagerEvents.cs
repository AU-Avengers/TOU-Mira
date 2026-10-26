using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.HnsImpostor;

namespace TownOfUs.Events.HnsImpostor;

public static class HiderMysticEvents
{
    [RegisterEvent(100000)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (@event.IsCancelled)
        {
            return;
        }
        var seeker = @event.Source;
        var victim = @event.Target;

        if (victim.TryRemoveModifier<HnsGlobalCamouflageModifier>() && victim.AmOwner)
        {
            seeker.TryRemoveModifier<HnsGlobalCamouflageModifier>();
        }
    }
}