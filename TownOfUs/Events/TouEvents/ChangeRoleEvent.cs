using MiraAPI.Events;

namespace TownOfUs.Events.TouEvents;

/// <summary>
///     Event that is invoked after a player's role is changed through Tou Mira. This event is not cancelable.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="ChangeRoleEvent" /> class.
/// </remarks>
/// <param name="player">The player.</param>
/// <param name="oldRole">The player's previous role.</param>
/// <param name="newRole">The player's new role.</param>
/// <param name="forceChange">Whether the role was selected and can override the previous role.</param>
public class ChangeRoleEvent(PlayerControl player, RoleBehaviour? oldRole, RoleBehaviour newRole, bool forceChange = false) : MiraEvent
{

    /// <summary>
    ///     Gets whether or not the role was overwritten by another system.
    /// </summary>
    public bool ForceChange { get; } = forceChange;

    /// <summary>
    ///     Gets the player that changed roles.
    /// </summary>
    public PlayerControl Player { get; } = player;

    /// <summary>
    ///     Gets the Role of the player prior to the role being changed.
    /// </summary>
    public RoleBehaviour? OldRole { get; } = oldRole;

    /// <summary>
    ///     Gets the Role of the player after the role is changed.
    /// </summary>
    public RoleBehaviour NewRole { get; } = newRole;
}