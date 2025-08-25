using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers;

// This modifier is used to stop a player from becoming Haunter, Phantom, or other tou ghost roles
public sealed class BasicGhostModifier : BaseModifier
{
    public override string ModifierName => "Basic Ghost";

    public override bool HideOnUi => true;
}