using MiraAPI.Events;

namespace TownOfUs.Events.TouEvents;

/// <summary>
///     Event that is invoked after a player's role is changed through Tou Mira. This event is not cancelable.
/// </summary>
public class ChangeRoleEvent : MiraEvent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChangeRoleEvent" /> class.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="oldRole">The player's previous role.</param>
    /// <param name="newRole">The player's new role.</param>
    public ChangeRoleEvent(PlayerControl player, RoleBehaviour? oldRole, RoleBehaviour newRole, bool forceChange = false)
    {
        Player = player;
        OldRole = oldRole;
        NewRole = newRole;
        ForceChange = forceChange;
    }

    /// <summary>
    ///     Gets whether or not the role was overwritten by another system.
    /// </summary>
    public bool ForceChange { get; }

    /// <summary>
    ///     Gets the player that changed roles.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    ///     Gets the Role of the player prior to the role being changed.
    /// </summary>
    public RoleBehaviour? OldRole { get; }

    /// <summary>
    ///     Gets the Role of the player after the role is changed.
    /// </summary>
    public RoleBehaviour NewRole { get; }
}