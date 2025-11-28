using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
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

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Mimic);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.GlitchMimic, Player, target);
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
        Player.ResetAppearance();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.GlitchUnmimic, Player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }
    }
}