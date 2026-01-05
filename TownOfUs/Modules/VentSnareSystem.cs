using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules;

public static class VentTrapSystem
{
    private sealed record TrapEntry(byte OwnerId, int RoundsRemaining);

    // ventId -> trap info
    private static readonly Dictionary<int, TrapEntry> _traps = new();

    public static bool TryGetTraprId(int ventId, out byte traprId)
    {
        if (_traps.TryGetValue(ventId, out var entry))
        {
            traprId = entry.OwnerId;
            return true;
        }

        traprId = default;
        return false;
    }

    public static bool IsTrapped(int ventId) => _traps.ContainsKey(ventId);

    public static void Place(int ventId, byte traprId)
    {
        var rounds = (int)OptionGroupSingleton<TrapperOptions>.Instance.TrapRoundsLast;
        _traps[ventId] = new TrapEntry(traprId, rounds);
    }

    public static void Remove(int ventId)
    {
        _traps.Remove(ventId);
    }

    public static void DecrementRoundsAndRemoveExpired()
    {
        var roundsLast = (int)OptionGroupSingleton<TrapperOptions>.Instance.TrapRoundsLast;
        if (roundsLast <= 0 || _traps.Count == 0)
        {
            return;
        }

        var toRemove = new List<int>();
        var toUpdate = new List<KeyValuePair<int, TrapEntry>>();

        foreach (var kvp in _traps)
        {
            var newRemaining = kvp.Value.RoundsRemaining - 1;
            if (newRemaining <= 0)
            {
                toRemove.Add(kvp.Key);
            }
            else
            {
                toUpdate.Add(new(kvp.Key, kvp.Value with { RoundsRemaining = newRemaining }));
            }
        }

        foreach (var ventId in toRemove)
        {
            _traps.Remove(ventId);
        }

        foreach (var kvp in toUpdate)
        {
            _traps[kvp.Key] = kvp.Value;
        }
    }

    public static void ClearAll()
    {
        _traps.Clear();
    }

    public static void ClearOwnedBy(byte traprId)
    {
        if (_traps.Count == 0)
        {
            return;
        }

        var toRemove = _traps.Where(kvp => kvp.Value.OwnerId == traprId).Select(kvp => kvp.Key).ToList();
        foreach (var ventId in toRemove)
        {
            _traps.Remove(ventId);
        }
    }

    public static bool IsEligibleToBeTrapped(PlayerControl pc)
    {
        if (pc == null || pc.HasDied())
        {
            return false;
        }

        var targets = OptionGroupSingleton<TrapperOptions>.Instance.TrapTargets;
        return targets switch
        {
            VentTrapTargets.Impostors => pc.IsImpostor(),
            VentTrapTargets.ImpostorsAndNeutrals => pc.IsImpostor() || pc.IsNeutral(),
            VentTrapTargets.All => true,
            _ => pc.IsImpostor() || pc.IsNeutral()
        };
    }

    public static Vector2 GetVentTopPosition(Vent vent)
    {
        // Matches the vanilla-ish “standing on vent” offset used elsewhere in the codebase.
        return (Vector2)vent.transform.position + new Vector2(0f, 0.3636f);
    }
}
