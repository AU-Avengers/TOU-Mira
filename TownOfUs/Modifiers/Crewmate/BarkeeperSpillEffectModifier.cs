using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class BarkeeperSpillEffectModifier(bool speedUp) : TimedModifier
{
    public override string ModifierName => "Spill Effect";
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<BarkeeperOptions>.Instance.SpillEffectDuration.Value;
    public bool SpeedUp = speedUp;
    public float Speed = 1f;

    public override void OnActivate()
    {
        base.OnActivate();
        var opts = OptionGroupSingleton<BarkeeperOptions>.Instance;
        Speed = SpeedUp ? opts.SpillEffectBuffMultiplier.Value : opts.SpillEffectDebuffMultiplier.Value;
    }
}