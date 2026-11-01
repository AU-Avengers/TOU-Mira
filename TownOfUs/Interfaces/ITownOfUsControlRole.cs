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
public interface ITownOfUsLocalControlRole : ITownOfUsControlRole, IController, IControlStarter
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
}

/// <summary>
/// Interface for classes that can initiate a remote-control interaction.
/// </summary>
public interface IControlStarter : IRemoteControl
{
    bool IsValidTarget(PlayerControl target)
    {
        return true;
    }

    IController SetupController(PlayerControl target);

    void SetupTarget(PlayerControl target);

    void CreateNotification();
}

/// <summary>
/// Interface for classes related to an already active remote-control interaction.
/// </summary>
public interface IRemoteControlActive : IRemoteControl
{
    void ClearNotification();

    /// <summary>
    /// The <see cref="PlayerControl"/> remote-controlling the modifier's player.
    /// </summary>
    PlayerControl Controller { get; }

    /// <summary>
    /// The <see cref="PlayerControl"/> being remote-controlled, if any.
    /// </summary>
    PlayerControl? Controlled { get; }

    bool ControllerCouldMove => false;

    bool ControllerCanMove => false;
}

/// <summary>
/// Interface for modifiers related to roles implementing <see cref="ITownOfUsControlRole"/>.
/// <para/>
/// Used specifically only for modifiers given to the victim of control.
/// </summary>
public interface IControlledModifier : IRemoteControlActive, IUncontrollable
{
    PlayerControl? IRemoteControlActive.Controlled => Player;

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
public interface IController : IRemoteControlActive
{
    PlayerControl IRemoteControlActive.Controller => Player;

    void UpdateOverlay() { }

    void ClearOnEnd();

    /// <summary>
    /// Called only on the <see cref="IRemoteControlActive.Controller"/>'s client ( <c>Controller.AmOwner</c> )
    /// to configure their local state when remote control interaction start or ends.
    /// </summary>
    void SetupLocal(bool onEnd) { }

    /// <summary>
    /// Called only on the <see cref="IRemoteControlActive.Controlled"/>'s client ( <c>Controlled.AmOwner</c> )
    /// to configure their local state when remote control interaction start or ends.
    /// </summary>
    /// <param name="onEnd"></param>
    void SetupTargetLocal(bool onEnd) { }

    /// <summary>
    /// Called to clear the <see cref="IRemoteControlActive.Controlled"/>'s physics state if they are found
    /// within a vent ( <c>Controlled.inVent</c> ) when the remote control interaction starts or ends.
    /// </summary>
    void ClearTargetInVent(bool onEnd)
    {
        Controlled?.MyPhysics.ExitAllVents();
    }

    /// <summary>
    /// Called after all other cleanup is done when the remote control interaction ends,
    /// to handle anything the base method did not already handle.
    /// </summary>
    void PostEndClear() { }
}