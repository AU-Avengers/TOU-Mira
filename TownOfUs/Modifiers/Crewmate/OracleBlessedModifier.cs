using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class OracleBlessedModifier(PlayerControl oracle) : BaseModifier
{
    public override string ModifierName => "Blessed";
    public override bool HideOnUi => true;
    public PlayerControl Oracle { get; } = oracle;

    public bool SavedFromExile { get; set; }

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.OracleBless, Oracle, Player);
        touAbilityEvent.InvokeEvent();
    }

    public override void OnDeath(DeathReason reason)
    {
        RemoveSelf();
    }
}