using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Roles.Neutral;

namespace TownOfUs.Modifiers;

public sealed class SerialKillerBloodlustModifier : TimedModifier
{
    public override float Duration => OptionGroupSingleton<SerialKillerOptions>.Instance.BloodlustDuration;
    public override string ModifierName => "Bloodlust";
    public override bool HideOnUi => true;
    public override bool Unique => true;

    public override string GetDescription()
    {
        return $"Your bloodlust is active, granting an additional kill button for {OptionGroupSingleton<SerialKillerOptions>.Instance.BloodlustDuration} second(s)!";
    }

}