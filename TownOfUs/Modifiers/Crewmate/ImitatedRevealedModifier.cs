using TownOfUs.Modules;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatedRevealedModifier(RoleBehaviour role)
    : RevealModifier((int)ChangeRoleResult.Nothing, true, role)
{
    public override string ModifierName => "Role Revealed";
    public override void OnActivate()
    {
        base.OnActivate();
        ChangeRoleResult = ChangeRoleResult.Nothing;
        var roleWhenAlive = Player.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType)
        {
            roleWhenAlive = crewType.CrewVariant;
        }
        SetNewInfo(true, null, null, roleWhenAlive);
    }
}