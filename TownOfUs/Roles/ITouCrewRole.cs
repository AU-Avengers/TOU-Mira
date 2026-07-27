using MiraAPI.Modifiers;

namespace TownOfUs.Roles;

public interface ITouCrewRole : ITownOfUsRole
{
    bool IsPowerCrew { get; }

    bool ITownOfUsRole.CanModifierContinueGame(BaseModifier modifier)
    {
        return true;
    }
}