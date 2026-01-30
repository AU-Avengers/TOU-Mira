using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers;

// This modifier is used to disable abilities on a player, and can be set up to disable on an interval.
// The modifier will disable all buttons, and can be set up to appear dead or unusable to certain roles with CanBeInteractWith or IsConsideredAlive
// A few examples of these are IsConcealed for the shield modifiers, and also transporter checks checking if a player is alive or cannot be transported
[MiraIgnore]
public abstract class DisabledModifier : TimedModifier
{
    public override string ModifierName => "Disabled Modifier";

    public virtual bool CanBeInteractedWith => true;
    public virtual bool IsConsideredAlive => true;
    public virtual bool CanUseAbilities => false;
    public virtual bool CanUseConsoles => CanUseAbilities;
    public virtual bool CanOpenMap => CanUseAbilities;
    public virtual bool CanReport => false;
    public override float Duration => 1f;
    public override bool AutoStart => false;
    public override bool HideOnUi => true;

    public override string GetDescription()
    {
        return "You are disabled!";
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        ModifierComponent!.RemoveModifier(this);
    }
}