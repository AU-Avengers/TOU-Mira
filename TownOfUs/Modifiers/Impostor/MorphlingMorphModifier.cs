using MiraAPI.Events;
using MiraAPI.GameOptions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor;

public sealed class MorphlingMorphModifier(PlayerControl target) : ConcealedModifier, IVisualAppearance
{
    public override float Duration => OptionGroupSingleton<MorphlingOptions>.Instance.MorphlingDuration;
    public override string ModifierName => "Morph";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public override bool VisibleToOthers => true;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MorphlingMorph, Player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        Player.ResetAppearance();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MorphlingUnmorph, Player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }
    }
}