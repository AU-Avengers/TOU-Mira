using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
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

    public PlayerControl Target { get; } = target;

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(Target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);

        // Visual-only: match First Death Shield appearance to the morphed target without granting the actual modifier.
        if (!Player.HasModifier<FirstDeadShield>() && Target.HasModifier<FirstDeadShield>() &&
            !Player.HasModifier<FirstDeadShieldDisguiseVisual>())
        {
            Player.AddModifier<FirstDeadShieldDisguiseVisual>(Target);
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MorphlingMorph, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (Player.HasModifier<FirstDeadShieldDisguiseVisual>())
        {
            Player.RemoveModifier<FirstDeadShieldDisguiseVisual>();
        }

        Player.ResetAppearance();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MorphlingUnmorph, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }
    }
}