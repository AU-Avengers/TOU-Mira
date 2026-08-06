using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Neutral;

public sealed class GlitchMimicModifier(PlayerControl target) : DisguisedModifier(target)
{
    public override float Duration => OptionGroupSingleton<GlitchOptions>.Instance.MimicDuration;
    public bool CanGlitchVent = true;

    protected override TownOfUsAppearances Appearance => TownOfUsAppearances.Mimic;

    public override void OnActivate()
    {
        CanGlitchVent =
            (GlitchVent)OptionGroupSingleton<GlitchOptions>.Instance.CanVent.Value is GlitchVent.Always;

        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.GlitchMimic, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            CustomButtonSingleton<GlitchMimicButton>.Instance.SetTimer(OptionGroupSingleton<GlitchOptions>.Instance
                .MimicCooldown);
        }

        base.OnDeactivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.GlitchUnmimic, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override bool? CanVent()
    {
        if (!CanGlitchVent)
        {
            return false;
        }

        return null;
    }
}