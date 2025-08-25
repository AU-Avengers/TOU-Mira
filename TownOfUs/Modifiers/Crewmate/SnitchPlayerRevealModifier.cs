namespace TownOfUs.Modifiers.Crewmate;

public sealed class SnitchPlayerRevealModifier(RoleBehaviour role)
    : RevealModifier((int)ChangeRoleResult.Nothing, true, role)
{
    public override string ModifierName => "Revealed Snitch";
}