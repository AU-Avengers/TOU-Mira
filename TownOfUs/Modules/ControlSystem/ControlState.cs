using System.Diagnostics.CodeAnalysis;
using TownOfUs.Utilities.ControlSystem;
using UnityEngine;

namespace TownOfUs.Modules.ControlSystem;

/// <summary>
/// Per-client state for cross-player-control. This is intentionally client-local:
/// - The controller sends the desired direction for a controlled player via RPC.
/// - The controlled player's owner applies that direction inside a movement patch.
/// </summary>
public class ControlState
{
    public static readonly Dictionary<Type, ControlState> Instances = [];

    public ControlState(Type type)
    {
        RemoteControlUtilities.EnsureControlRole(type);
        Instances.Add(type, this);
    }

    // After initial control begins, different clients may briefly disagree on transform state.
    // During this grace window we avoid applying any victim movement input to prevent desync.
    public const float InitialControlSyncGraceSeconds = 1.0f;

    private readonly Dictionary<byte, byte> ControlledBy = [];
    private readonly Dictionary<byte, byte> Controlling = [];
    protected readonly Dictionary<byte, bool> InControl = [];
    private readonly Dictionary<byte, Vector2> ControlledDirection = [];
    private readonly Dictionary<byte, Vector2> ControlledPosition = [];
    private readonly Dictionary<byte, Vector2> ControlledVelocity = [];
    private readonly Dictionary<byte, bool> ControlledInAnim = [];
    private readonly Dictionary<byte, float> ControlledSince = [];

    public virtual void SetControl(byte controlledId, byte controllerId)
    {
        ControlledBy[controlledId] = controllerId;
        Controlling[controllerId] = controlledId;

        InControl[controlledId] = false;
        InControl[controllerId] = true;

        ControlledDirection[controlledId] = Vector2.zero;
        ControlledPosition[controlledId] = Vector2.zero;
        ControlledVelocity[controlledId] = Vector2.zero;
        ControlledInAnim[controlledId] = false;
        ControlledSince[controlledId] = Time.time;
    }

    public virtual void ClearControl(byte controlledId)
    {
        ControlledBy.Remove(controlledId, out var controllerId);
        Controlling.Remove(controllerId);

        InControl.Remove(controlledId);
        InControl.Remove(controllerId);

        ControlledDirection.Remove(controlledId);
        ControlledPosition.Remove(controlledId);
        ControlledVelocity.Remove(controlledId);
        ControlledInAnim.Remove(controlledId);
        ControlledSince.Remove(controlledId);
    }

    public bool IsControlled(byte controlledId, out byte controllerId)
    {
        return ControlledBy.TryGetValue(controlledId, out controllerId);
    }

    public bool IsControlling(byte controllerId, out byte controlledId)
    {
        return Controlling.TryGetValue(controllerId, out controlledId);
    }

    public bool IsUsingState(byte playerId, out byte withId, out bool isControlled)
    {
        isControlled = IsControlled(playerId, out withId);
        return isControlled || IsControlling(playerId, out withId);
    }

    public bool HasControl(byte playerId)
    {
        return InControl.TryGetValue(playerId, out bool has) && has;
    }

    public bool IsFullyControlling(byte playerId)
    {
        return IsControlling(playerId, out byte otherId) && !HasControl(otherId);
    }

    public bool IsControllingActionable(byte playerId)
    {
        return IsControlling(playerId, out _) && HasControl(playerId);
    }

    public void SetForcedDirection(byte controlledId, Vector2 direction)
    {
        ControlledDirection[controlledId] = direction;
    }

    public Vector2 GetForcedDirection(byte controlledId)
    {
        return ControlledDirection.TryGetValue(controlledId, out var dir) ? dir : Vector2.zero;
    }

    public virtual Vector2 GetDirection(byte playerId)
    {
        return GetForcedDirection(playerId);
    }

    public void SetMovementState(byte controlledId, Vector2 position, bool inAnim, Vector2 velocity)
    {
        ControlledPosition[controlledId] = position;
        ControlledInAnim[controlledId] = inAnim;
        ControlledVelocity[controlledId] = velocity;
    }

    public Vector2 GetPosition(byte controlledId)
    {
        return ControlledPosition.TryGetValue(controlledId, out var pos) ? pos : Vector2.zero;
    }

    public bool IsInAnim(byte controlledId)
    {
        return ControlledInAnim.TryGetValue(controlledId, out var val) && val;
    }

    public Vector2 GetVelocity(byte controlledId)
    {
        return ControlledVelocity.TryGetValue(controlledId, out var vel) ? vel : Vector2.zero;
    }

    public float GetControlElapsedSeconds(byte controlledId)
    {
        return ControlledSince.TryGetValue(controlledId, out var since) ? Mathf.Max(0f, Time.time - since) : float.PositiveInfinity;
    }

    public bool IsInInitialGrace(byte controlledId)
    {
        return GetControlElapsedSeconds(controlledId) < InitialControlSyncGraceSeconds;
    }

    public static bool IsUsingAnyState(byte playerId, out byte otherId, out bool isControlled, [NotNullWhen(true)] out ControlState? controlState)
    {
        otherId = default;
        isControlled = false;
        controlState = null;

        foreach (var state in Instances.Values)
        {
            if (state.IsUsingState(playerId, out otherId, out isControlled))
            {
                controlState = state;
                return true;
            }
        }

        return false;
    }

    public static void ClearAllInstances()
    {
        foreach (var instance in Instances.Values)
        {
            instance.ClearAll();
        }
    }

    public virtual void ClearAll()
    {
        ControlledBy.Clear();
        Controlling.Clear();
        InControl.Clear();
        ControlledDirection.Clear();
        ControlledPosition.Clear();
        ControlledVelocity.Clear();
        ControlledInAnim.Clear();
        ControlledSince.Clear();
    }
}
