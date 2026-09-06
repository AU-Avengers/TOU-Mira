using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class DisguisedModifier(PlayerControl target) : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => Appearance.ToString();
    public override bool HideOnUi => true;
    public override bool VisibleToOthers => true;
    public override bool AutoStart => true;
    public bool VisualPriority => true;

    public PlayerControl Target { get; } = target;

    protected abstract TownOfUsAppearances Appearance { get; }

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(Target.GetDefaultModifiedAppearance(), Appearance);
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
    }

    public override void OnDeactivate()
    {
        if (Player.HasModifier<FirstDeadShieldDisguiseVisual>())
        {
            Player.RemoveModifier<FirstDeadShieldDisguiseVisual>();
        }

        Player.ResetAppearance();

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }
    }
}
