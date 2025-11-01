namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatedRevealedModifier(RoleBehaviour role)
    : RevealModifier((int)ChangeRoleResult.Nothing, true, role)
{
    public override string ModifierName => "Role Revealed";
}