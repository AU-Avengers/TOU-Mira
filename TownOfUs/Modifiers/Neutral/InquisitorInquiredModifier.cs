using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class InquisitorInquiredModifier : BaseModifier
{
    public override string ModifierName => "Inquired";
    public override bool HideOnUi => true;
}