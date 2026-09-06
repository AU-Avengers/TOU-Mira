using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MayorRevealModifier(RoleBehaviour role)
    : BaseRevealModifier
{
    public override string ModifierName => "Mayor Reveal";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Player.Data.Role is MayorRole mayor)
        {
            Visible = mayor.Revealed;
        }
        else
        {
            Visible = false;
        }
    }
}