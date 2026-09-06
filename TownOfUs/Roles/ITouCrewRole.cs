using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;

namespace TownOfUs.Roles;

public interface ITouCrewRole : ITownOfUsRole
{
    bool IsPowerCrew { get; }

    [HideFromIl2Cpp]
    bool ITownOfUsRole.CanModifierContinueGame(BaseModifier modifier)
    {
        return true;
    }
}