using MiraAPI.Events;

namespace TownOfUs.Events.TouEvents;

/// <summary>
///     Event that is invoked when the game ends for the local player. This event is not cancelable.
/// </summary>
public class ClientGameEndEvent : MiraEvent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientGameEndEvent" /> class.
    /// </summary>
    public ClientGameEndEvent()
    {
    }
}