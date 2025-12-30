using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules;

public static class VentSnareSystem
{
    // ventId -> snarerPlayerId
    private static readonly Dictionary<int, byte> _snares = new();

    public static bool TryGetSnarerId(int ventId, out byte snarerId) => _snares.TryGetValue(ventId, out snarerId);

    public static bool IsSnared(int ventId) => _snares.ContainsKey(ventId);

    public static void Place(int ventId, byte snarerId)
    {
        // Only allow one active snared vent per Snarer: placing a new snare replaces the old one.
        ClearOwnedBy(snarerId);
        _snares[ventId] = snarerId;
    }

    public static void Remove(int ventId)
    {
        _snares.Remove(ventId);
    }

    public static void ClearAll()
    {
        _snares.Clear();
    }

    public static void ClearOwnedBy(byte snarerId)
    {
        if (_snares.Count == 0)
        {
            return;
        }

        var toRemove = _snares.Where(kvp => kvp.Value == snarerId).Select(kvp => kvp.Key).ToList();
        foreach (var ventId in toRemove)
        {
            _snares.Remove(ventId);
        }
    }

    public static bool IsEligibleToBeSnared(PlayerControl pc)
    {
        return pc != null && !pc.HasDied() && (pc.IsImpostor() || pc.IsNeutral());
    }

    public static Vector2 GetVentTopPosition(Vent vent)
    {
        // Matches the vanilla-ish “standing on vent” offset used elsewhere in the codebase.
        return (Vector2)vent.transform.position + new Vector2(0f, 0.3636f);
    }
}