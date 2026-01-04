using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Modifiers.Impostor;

public sealed class PuppeteerHintArrowModifier(PlayerControl owner)
    : ArrowTargetModifier(owner, Palette.ImpostorRed, 0f)
{
    public override string ModifierName => "Hint Arrow";
    public override float Duration => OptionGroupSingleton<PuppeteerOptions>.Instance.VictimSeesControlDirection.Value;
    public override bool AutoStart => true;

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        TouAudio.PlaySound(TouAudio.TrackerDeactivateSound);
        base.OnDeath(reason);
    }
}