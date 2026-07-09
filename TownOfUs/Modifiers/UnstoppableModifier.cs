using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers;

public sealed class UnstoppableModifier : BaseModifier
{
    public override string ModifierName => "Unstoppable";
    public override bool HideOnUi => true;
}
