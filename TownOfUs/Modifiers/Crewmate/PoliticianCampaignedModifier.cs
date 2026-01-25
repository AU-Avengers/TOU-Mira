using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class PoliticianCampaignedModifier(PlayerControl politician) : BaseModifier
{
    public override string ModifierName => "Campaigned";
    public override bool HideOnUi => true;

    public PlayerControl Politician { get; } = politician;

    // Amne Politician won't break with this now
    public override bool Unique => false;

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.PoliticianCampaign, Politician, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}