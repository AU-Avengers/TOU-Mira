using TownOfUs.Modules.ControlSystem;
using TownOfUs.Roles;

namespace TownOfUs.Interfaces;

/// <summary>
/// Interface for roles that desire to interact with a <see cref="ControlState"/> and applicable remote-control utilities.
/// </summary>
public interface ITownOfUsControlRole : ITownOfUsRole
{
}

/// <summary>
/// Interface for roles that interact with a <see cref="ControlState"/> and locally track their own control, too.
/// </summary>
public interface ITownOfUsLocalControlRole : ITownOfUsControlRole
{
    void ClearControlLocal();
}