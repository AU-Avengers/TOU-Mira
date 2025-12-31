using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules;

public static class VentTrapSystem
{
    // ventId -> traprPlayerId
    private static readonly Dictionary<int, byte> _traps = new();

    public static bool TryGetTraprId(int ventId, out byte traprId) => _traps.TryGetValue(ventId, out traprId);

    public static bool IsTrapped(int ventId) => _traps.ContainsKey(ventId);

    public static void Place(int ventId, byte traprId)
    {
        // Only allow one active trapped vent per Trapper: placing a new trap replaces the old one.
        ClearOwnedBy(traprId);
        _traps[ventId] = traprId;
    }

    public static void Remove(int ventId)
    {
        _traps.Remove(ventId);
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

        var toRemove = _traps.Where(kvp => kvp.Value == traprId).Select(kvp => kvp.Key).ToList();
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