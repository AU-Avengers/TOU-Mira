using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using TownOfUs.Interfaces;

namespace TownOfUs.Modifiers;

// This modifier is used to prevent transports with anyone who has this
[MiraIgnore]
public abstract class NoTransportModifier : TimedModifier, IUntransportable
{
    public override string ModifierName => "Untransportable Modifier";

    public override float Duration => 1f;
    public override bool AutoStart => false;
    public override bool HideOnUi => true;
    public virtual bool VisibleToOthers { get; set; }

    public override string GetDescription()
    {
        return "You cannot be transported!";
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        ModifierComponent!.RemoveModifier(this);
    }
}