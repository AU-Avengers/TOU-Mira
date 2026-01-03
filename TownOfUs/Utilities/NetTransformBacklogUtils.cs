using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Utilities;

/// <summary>
/// Utilities for flushing buffered/interpolated state inside <see cref="CustomNetworkTransform"/>.
/// Used to prevent visual "replay" when we temporarily suppress CNT updates (e.g., Parasite control camera).
/// </summary>
internal static class NetTransformBacklogUtils
{
    private static bool _searched;
    private static FieldInfo[] _clearableCollectionFields = Array.Empty<FieldInfo>();

    private static void EnsureSearched()
    {
        if (_searched)
        {
            return;
        }

        _searched = true;

        try
        {
            var type = typeof(CustomNetworkTransform);
            var fields = AccessTools.GetDeclaredFields(type);

            var candidates = new List<FieldInfo>();
            foreach (var f in fields)
            {
                try
                {
                    var ft = f.FieldType;
                    if (ft == null)
                    {
                        continue;
                    }

                    var name = (f.Name ?? string.Empty).ToLowerInvariant();
                    var nameLooksLikeBuffer =
                        name.Contains("recv") ||
                        name.Contains("receive") ||
                        name.Contains("buffer") ||
                        name.Contains("queue") ||
                        name.Contains("snap") ||
                        name.Contains("lerp") ||
                        name.Contains("history");

                    var isQueueOrList =
                        (ft.IsGenericType &&
                         (ft.GetGenericTypeDefinition() == typeof(Queue<>) ||
                          ft.GetGenericTypeDefinition() == typeof(List<>))) ||
                        typeof(IList).IsAssignableFrom(ft) ||
                        typeof(ICollection).IsAssignableFrom(ft);

                    if (isQueueOrList && nameLooksLikeBuffer)
                    {
                        candidates.Add(f);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            _clearableCollectionFields = candidates.Distinct().ToArray();
        }
        catch
        {
            _clearableCollectionFields = Array.Empty<FieldInfo>();
        }
    }

    private static void TryClear(object obj)
    {
        if (obj == null)
        {
            return;
        }

        try
        {
            // Most collection types expose Clear().
#pragma warning disable S3011
            var clear = obj.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
#pragma warning restore S3011
            if (clear != null)
            {
                clear.Invoke(obj, null);
                return;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static FieldInfo? _isPausedField;

    private static FieldInfo? GetIsPausedField()
    {
        if (_isPausedField != null)
        {
            return _isPausedField;
        }

        try
        {
            var type = typeof(CustomNetworkTransform);
            _isPausedField = AccessTools.Field(type, "isPaused");
        }
        catch
        {
            // ignored
        }

        return _isPausedField;
    }

    /// <summary>
    /// Temporarily pauses CNT updates, flushes backlog, then resumes. This prevents visual snaps
    /// by ensuring no network updates are processed during the flush.
    /// </summary>
    public static void FlushBacklogWithPause(PlayerControl player)
    {
        if (player == null || player.NetTransform == null)
        {
            return;
        }

        var cnt = player.NetTransform.TryCast<CustomNetworkTransform>();
        if (cnt == null)
        {
            return;
        }

        var isPausedField = GetIsPausedField();
        bool wasPaused = false;

        // Temporarily pause CNT updates
        try
        {
            if (isPausedField != null)
            {
                wasPaused = (bool)(isPausedField.GetValue(cnt) ?? false);
                isPausedField.SetValue(cnt, true);
            }
        }
        catch
        {
            // ignored
        }

        // Flush the backlog
        FlushBacklog(player);

        // Resume CNT updates
        try
        {
            if (isPausedField != null && !wasPaused)
            {
                isPausedField.SetValue(cnt, false);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Clears buffered CNT state (best-effort) without snapping. Used to prevent replay of queued updates
    /// without causing visual snaps on other clients.
    /// </summary>
    public static void FlushBacklog(PlayerControl player)
    {
        if (player == null || player.NetTransform == null)
        {
            return;
        }

        var cnt = player.NetTransform.TryCast<CustomNetworkTransform>();
        if (cnt == null)
        {
            return;
        }

        EnsureSearched();

        try
        {
            for (var i = 0; i < _clearableCollectionFields.Length; i++)
            {
                var f = _clearableCollectionFields[i];
                var v = f.GetValue(cnt);
                if (v != null)
                {
                    TryClear(v);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Clears buffered CNT state (best-effort) and snaps the transform so it doesn't replay queued updates.
    /// </summary>
    public static void FlushAndSnap(PlayerControl player)
    {
        if (player == null || player.NetTransform == null)
        {
            return;
        }

        var cnt = player.NetTransform.TryCast<CustomNetworkTransform>();
        if (cnt == null)
        {
            // Fall back to a plain snap
            try
            {
                player.NetTransform.SnapTo(player.transform.position);
            }
            catch
            {
                // ignored
            }
            return;
        }

        EnsureSearched();

        try
        {
            for (var i = 0; i < _clearableCollectionFields.Length; i++)
            {
                var f = _clearableCollectionFields[i];
                var v = f.GetValue(cnt);
                if (v != null)
                {
                    TryClear(v);
                }
            }
        }
        catch
        {
            // ignored
        }

        var pos = (Vector2)player.transform.position;
        try
        {
            // Bump sequence id to force the CNT to treat this as a fresh authoritative snap locally.
            cnt.SnapTo(pos, (ushort)(cnt.lastSequenceId + 1));
        }
        catch
        {
            try
            {
                player.NetTransform.SnapTo(pos);
            }
            catch
            {
                // ignored
            }
        }
    }
}