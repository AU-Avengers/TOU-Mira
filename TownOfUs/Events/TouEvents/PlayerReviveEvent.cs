using MiraAPI.Events;

namespace TownOfUs.Events.TouEvents;

/// <summary>
///     Event that is invoked after a player is revived. This event is not cancelable.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PlayerReviveEvent" /> class.
/// </remarks>
/// <param name="player">The player who was revived.</param>
public class PlayerReviveEvent(PlayerControl player) : MiraEvent
{

    /// <summary>
    ///     Gets the player who was revived.
    /// </summary>
    public PlayerControl Player { get; } = player;
}