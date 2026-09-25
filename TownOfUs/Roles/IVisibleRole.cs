namespace TownOfUs.Roles;

public interface IVisibleRole
{
    bool CanOtherRoleSee(RoleBehaviour role, out bool consideredTeammates);
}