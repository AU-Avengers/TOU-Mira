using MiraAPI.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor;

public sealed class ShapeshifterShiftModifier(PlayerControl target) : DisguisedModifier(target)
{
    // This doesn't autostart, as we let vanilla handle the timer logic instead.
    public override bool AutoStart => false;

    protected override TownOfUsAppearances Appearance => TownOfUsAppearances.Shapeshifted;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.ShapeshifterShift, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.ShapeshifterUnshift, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}