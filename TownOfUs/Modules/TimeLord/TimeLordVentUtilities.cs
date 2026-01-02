using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace TownOfUs.Modules.TimeLord;

/// <summary>
/// Utilities for vent-related operations in Time Lord rewind system.
/// </summary>
internal static class TimeLordVentUtilities
{
    private static FieldInfo? _targetVentField;
    private static bool _targetVentFieldSearched;

    private static FieldInfo? GetTargetVentField()
    {
        if (_targetVentFieldSearched)
        {
            return _targetVentField;
        }

        _targetVentFieldSearched = true;

        static FieldInfo? FindOnType(Type t)
        {
            try
            {
                foreach (var f in AccessTools.GetDeclaredFields(t))
                {
                    if (f.FieldType != typeof(Vent))
                    {
                        continue;
                    }

                    var n = (f.Name ?? string.Empty).ToLowerInvariant();
                    if (n.Contains("vent") && (n.Contains("target") || n.Contains("enter") || n.Contains("use")))
                    {
                        return f;
                    }
                }

                foreach (var f in AccessTools.GetDeclaredFields(t))
                {
                    if (f.FieldType == typeof(Vent) && (f.Name ?? string.Empty).ToLowerInvariant().Contains("vent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return f;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        _targetVentField = FindOnType(typeof(PlayerControl)) ?? FindOnType(typeof(PlayerPhysics));
        return _targetVentField;
    }

    public static int TryGetVentIdFromPlayerState(PlayerControl lp, PlayerPhysics? physics)
    {
        if (Vent.currentVent != null)
        {
            return Vent.currentVent.Id;
        }

        var f = GetTargetVentField();
        if (f != null)
        {
            try
            {
                Vent? v;
                if (f.DeclaringType == typeof(PlayerPhysics))
                {
                    v = physics != null ? f.GetValue(physics) as Vent : null;
                }
                else
                {
                    v = f.GetValue(lp) as Vent;
                }

                if (v != null)
                {
                    return v.Id;
                }
            }
            catch
            {
                // ignored
            }
        }

        if (ShipStatus.Instance?.AllVents != null)
        {
            try
            {
                var p = (Vector2)lp.transform.position;
                Vent? best = null;
                var bestD2 = float.MaxValue;
                foreach (var v in ShipStatus.Instance.AllVents)
                {
                    if (v == null) continue;
                    var d2 = ((Vector2)v.transform.position - p).sqrMagnitude;
                    if (d2 < bestD2)
                    {
                        bestD2 = d2;
                        best = v;
                    }
                }

                if (best != null && bestD2 <= 2.0f * 2.0f)
                {
                    return best.Id;
                }
            }
            catch
            {
                // ignored
            }
        }

        return -1;
    }

    public static Vent? GetVentById(int id)
    {
        if (id < 0 || ShipStatus.Instance == null || ShipStatus.Instance.AllVents == null)
        {
            return null;
        }

        try
        {
            return ShipStatus.Instance.AllVents.FirstOrDefault(x => x != null && x.Id == id);
        }
        catch
        {
            return null;
        }
    }

    public static void ApplyVentSnapshotState(PlayerControl lp, Snapshot snap)
    {
        var wantInVent = (snap.Flags & SnapshotState.InVent) != 0;

        if (wantInVent)
        {
            if (snap.VentId < 0)
            {
                // Vent ID is invalid, cannot enter vent
            }
            else
            {
                var v = GetVentById(snap.VentId);
                if (v == null)
                {
                    // Vent not found, cannot enter vent
                }
                else
                {
                    Vent.currentVent = v;

                    if (!lp.inVent)
                    {
                        try { lp.MyPhysics?.RpcEnterVent(v.Id); } catch { /* ignored */ }
                    }

                    lp.inVent = true;
                    lp.walkingToVent = false;
                    return;
                }
            }
        }

        if (lp.inVent || Vent.currentVent != null)
        {
            try
            {
                var v = Vent.currentVent ?? (snap.VentId >= 0 ? GetVentById(snap.VentId) : null);
                if (v != null)
                {
                    try { v.SetButtons(false); } catch { /* ignored */ }
                    try { lp.MyPhysics?.RpcExitVent(v.Id); } catch { /* ignored */ }
                }

                lp.MyPhysics?.ExitAllVents();
            }
            catch
            {
                // ignored
            }
        }

        lp.inVent = false;
        Vent.currentVent = null;
        lp.walkingToVent = (snap.Flags & SnapshotState.WalkingToVent) != 0;
    }
}