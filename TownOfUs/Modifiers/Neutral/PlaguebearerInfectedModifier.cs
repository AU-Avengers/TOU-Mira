using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Modifiers.Neutral;

public sealed class PlaguebearerInfectedModifier(byte plaguebearerId) : BaseModifier
{
    public override string ModifierName => "Infected";
    public override bool HideOnUi => true;

    public byte PlagueBearerId { get; } = plaguebearerId;

    public override void OnActivate()
    {
        base.OnActivate();

        var pb = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == PlagueBearerId);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.PlaguebearerInfect, pb!, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}