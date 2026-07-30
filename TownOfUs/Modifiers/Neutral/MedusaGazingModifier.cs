using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Neutral;

namespace TownOfUs.Modifiers.Neutral;

public sealed class MedusaGazingModifier : TimedModifier
{
    public override float Duration => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeDuration.Value;
    public override string ModifierName => "Gazing";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MedusaGazing, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }
}