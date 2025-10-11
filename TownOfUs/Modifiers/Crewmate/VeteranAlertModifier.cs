using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class VeteranAlertModifier : TimedModifier
{
    public override float Duration => OptionGroupSingleton<VeteranOptions>.Instance.AlertDuration;
    public override string ModifierName => "Alerted";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.VeteranAlert, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (Player.Data.Role is VeteranRole vet)
        {
            vet.Alerts--;
        }
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