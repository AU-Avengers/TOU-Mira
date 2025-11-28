using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Neutral;

namespace TownOfUs.Modifiers.Neutral;

public sealed class SurvivorVestModifier : TimedModifier
{
    public override float Duration => OptionGroupSingleton<SurvivorOptions>.Instance.VestDuration;
    public override string ModifierName => "Vested";
    public override bool AutoStart => true;
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.SurvivorVest, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}