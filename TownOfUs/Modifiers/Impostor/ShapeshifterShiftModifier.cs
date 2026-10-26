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
        touAbilityEvent.InvokeEvent();
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        RemoveSelf();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.ShapeshifterUnshift, Player, Target);
        touAbilityEvent.InvokeEvent();
    }
}