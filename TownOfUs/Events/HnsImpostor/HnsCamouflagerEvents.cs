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

        if (victim.TryGetModifier<HnsGlobalCamouflageModifier>(out var camoMod))
        {
            victim.RemoveModifier(camoMod);

            if (victim.AmOwner && seeker.TryGetModifier<HnsGlobalCamouflageModifier>(out var camoMod2))
            {
                seeker.RemoveModifier(camoMod2);
            }
        }
    }
}