using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers;

// This modifier is used to hide player animations and whatnot, very useful for the shield modifiers
[MiraIgnore]
public abstract class ConcealedModifier : TimedModifier
{
    public override string ModifierName => "Concealed Modifier";

    public override float Duration => 1f;
    public override bool AutoStart => false;
    public override bool HideOnUi => true;
    public virtual bool VisibleToOthers { get; set; }
    public virtual bool CarriesIntoMeetings => false;

    public override string GetDescription()
    {
        return "You are concealed!";
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        if (!CarriesIntoMeetings)
        {
            ModifierComponent!.RemoveModifier(this);
        }
    }
}
