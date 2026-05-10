using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Modifiers.Impostor.Venerer;

public sealed class VenererSprintModifier : TimedModifier, IVenererModifier
{
    public override string ModifierName => "Sprint";
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.VenererSprintAbility, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}