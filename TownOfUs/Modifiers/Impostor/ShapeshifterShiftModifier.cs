using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor;

public sealed class ShapeshifterShiftModifier(PlayerControl target) : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Shifted";
    public override bool HideOnUi => true;
    // This doesn't autostart, as we let vanilla handle the timer logic instead.
    public override bool AutoStart => false;
    public override bool VisibleToOthers => true;
    public bool VisualPriority => true;

    public PlayerControl Target { get; } = target;

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(Target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Shapeshifted);
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

        var touAbilityEvent = new TouAbilityEvent(AbilityType.ShapeshifterShift, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (Player.HasModifier<FirstDeadShieldDisguiseVisual>())
        {
            Player.RemoveModifier<FirstDeadShieldDisguiseVisual>();
        }

        Player.ResetAppearance();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.ShapeshifterUnshift, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }
    }
}