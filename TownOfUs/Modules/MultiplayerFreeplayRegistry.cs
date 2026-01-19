using TownOfUs.Utilities;

namespace TownOfUs.Modules;

/// <summary>
/// Provides a stable, deterministic mapping from modifier types to small ids
/// so Multiplayer Freeplay RPCs don't need to send strings.
/// </summary>
public static class MultiplayerFreeplayRegistry
{
    private static List<Type>? _modifierTypesSorted;
    private static Dictionary<Type, ushort>? _modifierTypeToId;

    public static void EnsureInitialized()
    {
        if (_modifierTypesSorted != null && _modifierTypeToId != null)
        {
            return;
        }

        // Deterministic across clients as long as the loaded mod set is identical.
        _modifierTypesSorted = MiscUtils.AllModifiers
            .Where(m => m != null)
            .Select(m => m.GetType())
            .Distinct()
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        _modifierTypeToId = new Dictionary<Type, ushort>(_modifierTypesSorted.Count);
        for (ushort i = 0; i < _modifierTypesSorted.Count; i++)
        {
            _modifierTypeToId[_modifierTypesSorted[i]] = i;
        }
    }

    public static bool TryGetModifierId(Type modifierType, out ushort id)
    {
        EnsureInitialized();
        if (_modifierTypeToId != null && _modifierTypeToId.TryGetValue(modifierType, out id))
        {
            return true;
        }

        id = 0;
        return false;
    }

    public static bool TryGetModifierType(ushort id, out Type? modifierType)
    {
        EnsureInitialized();
        if (_modifierTypesSorted != null && id < _modifierTypesSorted.Count)
        {
            modifierType = _modifierTypesSorted[id];
            return true;
        }

        modifierType = null;
        return false;
    }

    public static bool IsModifierAddableWithoutParameters(Type modifierType)
    {
        return modifierType.GetConstructor(Type.EmptyTypes) != null;
    }
}