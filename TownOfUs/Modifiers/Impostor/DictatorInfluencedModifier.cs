using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Impostor;

public sealed class DictatorInfluencedModifier(byte dictatorId) : BaseModifier
{
    public override string ModifierName => "Influenced";
    public override bool HideOnUi => true;
    public byte DictatorId { get; } = dictatorId;

    public override void OnActivate()
    {
        base.OnActivate();

        var dictator = MiscUtils.PlayerById(DictatorId);
        if (dictator == null)
        {
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.DictatorInfluence, dictator, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
