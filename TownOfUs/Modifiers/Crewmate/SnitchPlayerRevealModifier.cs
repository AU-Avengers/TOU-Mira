namespace TownOfUs.Modifiers.Crewmate;

public sealed class SnitchPlayerRevealModifier(RoleBehaviour role)
    : BaseRevealModifier
{
    public override string ModifierName => "Revealed Snitch";

    public ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public RoleBehaviour? ShownRole { get; set; } = role;
    public bool RevealRole { get; set; } = true;
}