using MiraAPI.Events;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Modifiers.Neutral;

public sealed class MercenaryGuardModifier(PlayerControl mercenary) : TimedModifier
{
    public override float Duration => 2.5f;
    public override bool AutoStart => false;
    public override string ModifierName => "Mercenary Guard";
    public override bool HideOnUi => true;
    public PlayerControl Mercenary { get; } = mercenary;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MercenaryGuard, Mercenary, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}