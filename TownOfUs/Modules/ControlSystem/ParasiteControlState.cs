using UnityEngine;

namespace TownOfUs.Modules.ControlSystem;

/// <summary>
/// Per-client state for Parasite control. This is intentionally client-local:
/// - The Parasite sends the desired direction for a controlled player via RPC.
/// - The controlled player's owner applies that direction inside a movement patch.
/// </summary>
public static class ParasiteControlState
{
    private static readonly Dictionary<byte, byte> ControlledBy = new();
    private static readonly Dictionary<byte, Vector2> ControlledDirection = new();
    private static readonly Dictionary<byte, Vector2> ControlledPosition = new();
    private static readonly Dictionary<byte, Vector2> ControlledVelocity = new();

    public static void SetControl(byte controlledId, byte controllerId)
    {
        ControlledBy[controlledId] = controllerId;
        ControlledDirection[controlledId] = Vector2.zero;
        ControlledPosition[controlledId] = Vector2.zero;
        ControlledVelocity[controlledId] = Vector2.zero;
    }

    public static void ClearControl(byte controlledId)
    {
        ControlledBy.Remove(controlledId);
        ControlledDirection.Remove(controlledId);
        ControlledPosition.Remove(controlledId);
        ControlledVelocity.Remove(controlledId);
    }

    public static bool IsControlled(byte controlledId, out byte controllerId)
    {
        return ControlledBy.TryGetValue(controlledId, out controllerId);
    }

    public static void SetDirection(byte controlledId, Vector2 direction)
    {
        ControlledDirection[controlledId] = direction;
    }

    public static Vector2 GetDirection(byte controlledId)
    {
        return ControlledDirection.TryGetValue(controlledId, out var dir) ? dir : Vector2.zero;
    }

    public static void SetMovementState(byte controlledId, Vector2 position, Vector2 velocity)
    {
        ControlledPosition[controlledId] = position;
        ControlledVelocity[controlledId] = velocity;
    }

    public static Vector2 GetPosition(byte controlledId)
    {
        return ControlledPosition.TryGetValue(controlledId, out var pos) ? pos : Vector2.zero;
    }

    public static Vector2 GetVelocity(byte controlledId)
    {
        return ControlledVelocity.TryGetValue(controlledId, out var vel) ? vel : Vector2.zero;
    }

    public static void ClearAll()
    {
        ControlledBy.Clear();
        ControlledDirection.Clear();
        ControlledPosition.Clear();
        ControlledVelocity.Clear();
    }
}