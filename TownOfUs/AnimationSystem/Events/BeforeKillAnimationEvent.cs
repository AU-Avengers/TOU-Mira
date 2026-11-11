using MiraAPI.Events;

namespace TownOfUs.AnimationSystem.Events;

/// <summary>
/// Event that fires before a kill animation is fully loaded. To prevent the game's main logic from taking over, set a FinalAnimation.
/// </summary>
public sealed class BeforeKillAnimationEvent : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="NetworkedPlayerInfo"/> of the victim. Normally just the local player but may differ.
    /// </summary>
    public NetworkedPlayerInfo Victim { get; }

    /// <summary>
    /// Gets the <see cref="NetworkedPlayerInfo"/> of the killer.
    /// </summary>
    public NetworkedPlayerInfo Killer { get; }

    /// <summary>
    /// Gets or sets a value indicating what the final animation will end up being. If null, then the game will set up the animation instead.
    /// </summary>
    public OverlayKillAnimation? FinalAnimation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to alter the animation via roles or camo comms after the fact.
    /// </summary>
    public bool ShouldAlterAnimationAfter { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeKillAnimationEvent"/> class.
    /// </summary>
    /// <param name="victim">The <see cref="NetworkedPlayerInfo"/> of the victim.</param>
    /// <param name="killer">The <see cref="NetworkedPlayerInfo"/> of the killer.</param>
    public BeforeKillAnimationEvent(NetworkedPlayerInfo victim, NetworkedPlayerInfo killer)
    {
        Victim = victim;
        Killer = killer;
    }
}