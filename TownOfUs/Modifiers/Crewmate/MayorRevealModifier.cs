using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MayorRevealModifier(RoleBehaviour role)
    : BaseRevealModifier
{
    public override string ModifierName => "Mayor Reveal";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.RemoveModifier;

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent?.RemoveModifier(this);
    }

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