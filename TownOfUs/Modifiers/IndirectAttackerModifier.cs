using MiraAPI.Modifiers.Types;

namespace TownOfUs.Modifiers;

public sealed class IndirectAttackerModifier(bool ignoreShield) : TimedModifier
{
    public override string ModifierName => "Indirect Attacker";
    public override bool HideOnUi => true;
    public bool IgnoreShield => ignoreShield;
    public override float Duration => 1f;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}