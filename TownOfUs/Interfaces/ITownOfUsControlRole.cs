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
public interface ITownOfUsLocalControlRole : ITownOfUsControlRole, IController
{
    void ClearControlLocal();

    void IController.ClearOnEnd()
    {
        ClearControlLocal();
    }
}



/// <summary>
/// Interface for classes with a <see cref="PlayerControl"/> that will participate in
/// a remote-control interaction, with access to its <see cref="ControlState"/>.
/// </summary>
public interface IRemoteControl
{
    /// <summary>
    /// The current <see cref="PlayerControl"/> in the remote control interaction.
    /// </summary>
    PlayerControl Player { get; }

    /// <summary>
    /// The <see cref="ControlState"/> the interation should use.
    /// </summary>
    ControlState State { get; }
    
    void ClearNotification();
}

/// <summary>
/// Interface for modifiers related to roles implementing <see cref="ITownOfUsControlRole"/>.
/// <para/>
/// Used specifically only for modifiers given to the victim of control.
/// </summary>
public interface IControlledModifier : IRemoteControl, IUncontrollable
{
    /// <summary>
    /// The <see cref="PlayerControl"/> remote-controlling the modifier's player.
    /// </summary>
    PlayerControl Controller { get; }

    /// <summary>
    /// Whether the <see cref="Controller"/> is dead while controlling.
    /// Return <see langword="null"/> if they can be either alive or dead.
    /// </summary>
    bool? ControllerIsDead => false;

    void RemoveSelf();

    void UpdateOverlayLayout() { }
}

/// <summary>
/// Interface for classes that master the remote-control interaction.
/// </summary>
public interface IController : IRemoteControl
{
    /// <summary>
    /// The <see cref="PlayerControl"/> being remote-controlled, if any.
    /// </summary>
    PlayerControl? Controlled { get; }

    void UpdateOverlay() { }

    void ClearOnEnd();

    /// <summary>
    /// Called only on the <paramref name="controller"/>'s client ( <c>controller.AmOwner</c> )
    /// to configure their local state when remote control interaction ends.
    /// </summary>
    void SetupLocalOnClear(PlayerControl controller) { }

    /// <summary>
    /// Called to clear the <see cref="Controlled"/>'s physics state if they are found
    /// within a vent ( <c>Controlled.inVent</c> ) when the remote control interaction ends.
    /// </summary>
    void ClearTargetInVent() { }

    /// <summary>
    /// Called after all other cleanup is done when the remote control interaction ends,
    /// to handle anything the base method did not already handle.
    /// </summary>
    void PostEndClear() { }
}