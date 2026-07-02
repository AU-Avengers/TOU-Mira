using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class DeputyCampedModifier(PlayerControl deputy) : BaseModifier
{
    public override string ModifierName => "Camped";
    public override bool HideOnUi => true;

    public PlayerControl Deputy { get; } = deputy;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.DeputyCamp, Deputy, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Player || Deputy == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (Deputy.AmOwner)
        {
            Player?.cosmetics.SetOutline(true, (TownOfUsColors.Deputy));
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.cosmetics.SetOutline(false, (TownOfUsColors.Deputy));
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, (TownOfUsColors.Deputy));
    }
}