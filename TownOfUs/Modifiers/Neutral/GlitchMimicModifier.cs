using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Neutral;

public sealed class GlitchMimicModifier(PlayerControl target) : ConcealedModifier, IVisualAppearance
{
    public override float Duration => OptionGroupSingleton<GlitchOptions>.Instance.MimicDuration;
    public override string ModifierName => "Mimic";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public override bool VisibleToOthers => true;
    public bool VisualPriority => true;

    public PlayerControl Target { get; } = target;

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(Target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Mimic);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);

        // Visual-only: match First Death Shield appearance to the mimicked target without granting the actual modifier.
        if (!Player.HasModifier<FirstDeadShield>() && Target.HasModifier<FirstDeadShield>() &&
            !Player.HasModifier<FirstDeadShieldDisguiseVisual>())
        {
            Player.AddModifier<FirstDeadShieldDisguiseVisual>(Target);
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.GlitchMimic, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        CustomButtonSingleton<GlitchMimicButton>.Instance.SetTimer(OptionGroupSingleton<GlitchOptions>.Instance
            .MimicCooldown);

        if (Player.HasModifier<FirstDeadShieldDisguiseVisual>())
        {
            Player.RemoveModifier<FirstDeadShieldDisguiseVisual>();
        }

        Player.ResetAppearance();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.GlitchUnmimic, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }
    }
}