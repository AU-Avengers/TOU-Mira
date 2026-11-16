using MiraAPI.Events;

namespace TownOfUs.AnimationSystem.Events;

/// <summary>
/// Event that fires after a kill animation is fully loaded. This can be cancelled to prevent the vanilla logic to run.
/// </summary>
public sealed class AfterKillAnimationEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets a value indicating whether to apply changes from roles or camo comms. This can be checked for other purposes if needed.
    /// </summary>
    public bool ShouldAlterFurther { get; }

    /// <summary>
    /// Gets the <see cref="NetworkedPlayerInfo"/> of the victim. Normally just the local player but may differ.
    /// </summary>
    public NetworkedPlayerInfo Victim { get; }

    /// <summary>
    /// Gets the <see cref="NetworkedPlayerInfo"/> of the killer.
    /// </summary>
    public NetworkedPlayerInfo Killer { get; }

    /// <summary>
    /// Gets a value for the animation's data.
    /// </summary>
    public KillOverlayInitData InitData { get; }

    /// <summary>
    /// Gets or sets a value indicating the finalized animation.
    /// </summary>
    public OverlayKillAnimation Animation { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AfterKillAnimationEvent"/> class.
    /// </summary>
    /// <param name="victim">The <see cref="NetworkedPlayerInfo"/> of the victim.</param>
    /// <param name="killer">The <see cref="NetworkedPlayerInfo"/> of the killer.</param>
    /// <param name="anim">The <see cref="OverlayKillAnimation"/> to play.</param>
    /// <param name="shouldAlter">A value indicating whether to modify the animation further.</param>
    public AfterKillAnimationEvent(NetworkedPlayerInfo victim, NetworkedPlayerInfo killer, OverlayKillAnimation anim, KillOverlayInitData initData, bool shouldAlter)
    {
        Victim = victim;
        Killer = killer;
        Animation = anim;
        InitData =  initData;
        ShouldAlterFurther = shouldAlter;
    }
}