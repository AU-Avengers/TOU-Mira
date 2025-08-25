using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MayorRevealModifier(RoleBehaviour role)
    : RevealModifier((int)ChangeRoleResult.RemoveModifier, true, role)
{
    public override string ModifierName => "Mayor Reveal";

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