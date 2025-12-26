using BepInEx.Logging;
using MiraAPI.GameOptions;
using Reactor.Utilities;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules.TimeLord;

/// <summary>
/// Manages body cleaning and restoration for Time Lord rewind system.
/// </summary>
public static class TimeLordBodyManager
{
    public enum CleanedBodySource : byte
    {
        Unknown = 0,
        Rotting = 1,
        Janitor = 2,
    }

    public sealed class CleanedBodyRecord
    {
        public byte BodyId { get; }
        public Vector3 Position { get; set; }
        public DateTime TimeUtc { get; set; }
        public float TimeSeconds { get; set; }
        public DeadBody? Body { get; set; }
        public CleanedBodySource Source { get; set; }
        public bool Restored { get; set; }
        public bool RestoredThisRewind { get; set; }
        public string? OriginalPetId { get; set; }
        public bool PetWasRemoved { get; set; }
        public byte? DraggedByPlayerId { get; set; }

        public CleanedBodyRecord(byte bodyId, Vector3 pos, DateTime timeUtc, float timeSeconds, DeadBody? body)
        {
            BodyId = bodyId;
            Position = pos;
            TimeUtc = timeUtc;
            TimeSeconds = timeSeconds;
            Body = body;
            Restored = false;
            RestoredThisRewind = false;
            Source = CleanedBodySource.Unknown;
            OriginalPetId = null;
            PetWasRemoved = false;
            DraggedByPlayerId = null;
        }
    }

    private static readonly Dictionary<byte, CleanedBodyRecord> CleanedBodies = new();

    public static readonly ManualLogSource BodyLogger =
        BepInEx.Logging.Logger.CreateLogSource("TOU.TimeLordBodies");

    private static void LogBodyRestore(string msg)
    {
        var full = $"[TimeLordBodies] {msg}";
        try
        {
            BodyLogger.LogError(full);
        }
        catch
        {
            // ignored
        }

        try
        {
            UnityEngine.Debug.LogError(full);
        }
        catch
        {
            // ignored
        }
    }

    public static void RecordBodyCleaned(DeadBody body, CleanedBodySource source)
    {
        if (body == null)
        {
            return;
        }

        LogBodyRestore(
            $"RecordBodyCleaned: body={body.ParentId} active={body.gameObject != null && body.gameObject.activeSelf} pos={body.transform.position} timeSeconds={Time.time:0.000} timeUtc={DateTime.UtcNow:O} source={source}");

        // Check if body was being dragged by Undertaker
        byte? draggedBy = null;
        foreach (var drag in MiraAPI.Modifiers.ModifierUtils.GetActiveModifiers<TownOfUs.Modifiers.Impostor.DragModifier>())
        {
            if (drag.BodyId == body.ParentId)
            {
                draggedBy = drag.Player.PlayerId;
                break;
            }
        }

        CleanedBodies[body.ParentId] = new CleanedBodyRecord(
            body.ParentId,
            body.transform.position,
            DateTime.UtcNow,
            Time.time,
            body)
        {
            Restored = false,
            RestoredThisRewind = false,
            Source = source,
            DraggedByPlayerId = draggedBy
        };
    }

    public static void RestoreCleanedBody(byte bodyId)
    {
        if (!CleanedBodies.TryGetValue(bodyId, out var rec))
        {
            LogBodyRestore($"RestoreCleanedBody: body={bodyId} no record (cleanedBodiesTotal={CleanedBodies.Count})");
            return;
        }

        LogBodyRestore(
            $"RestoreCleanedBody: body={bodyId} record(pos={rec.Position}, timeSeconds={rec.TimeSeconds:0.000}, timeUtc={rec.TimeUtc:O}, restored={rec.Restored}, restoredThisRewind={rec.RestoredThisRewind}, source={rec.Source})");

        var body = FindDeadBodyIncludingInactive(bodyId);

        if (body == null || body.gameObject == null)
        {
            body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == bodyId);
        }

        if (body == null || body.gameObject == null)
        {
            LogBodyRestore(
                $"RestoreCleanedBody: body={bodyId} FAILED to find DeadBody object (recordPos={rec.Position}, cleanedBodiesTotal={CleanedBodies.Count})");
            return;
        }

        rec.Body = body;
        rec.Restored = true;
        if (TimeLordRewindSystem.IsRewinding)
        {
            rec.RestoredThisRewind = true;
        }

        if (body.gameObject.activeSelf)
        {
            LogBodyRestore($"RestoreCleanedBody: body={bodyId} already active; forcing alpha=1");
            foreach (var r in body.bodyRenderers)
            {
                if (r != null)
                {
                    r.color = r.color.SetAlpha(1f);
                }
            }

            if (rec.PetWasRemoved && !string.IsNullOrEmpty(rec.OriginalPetId))
            {
                var player = MiscUtils.PlayerById(bodyId);
                if (player != null && !player.AmOwner)
                {
                    player.SetPet(rec.OriginalPetId);
                    Coroutines.Start(CoRefreshPetState(player));
                    BodyLogger?.LogError($"[RestoreCleanedBody] Restored pet '{rec.OriginalPetId}' to player {bodyId} (body already active)");
                }
            }
            return;
        }

        body.transform.position = rec.Position;
        LogBodyRestore($"RestoreCleanedBody: body={bodyId} activating at pos={rec.Position}");

        body.gameObject.SetActive(true);

        foreach (var r in body.bodyRenderers)
        {
            if (r != null)
            {
                r.color = r.color.SetAlpha(1f);
            }
        }

        if (rec.PetWasRemoved && !string.IsNullOrEmpty(rec.OriginalPetId))
        {
            var player = MiscUtils.PlayerById(bodyId);
            if (player != null && !player.AmOwner)
            {
                player.SetPet(rec.OriginalPetId);
                Coroutines.Start(CoRefreshPetState(player));
                BodyLogger?.LogError($"[RestoreCleanedBody] Restored pet '{rec.OriginalPetId}' to player {bodyId}");
            }
        }

        // Restore Undertaker drag state if body was being dragged when cleaned
        if (rec.DraggedByPlayerId.HasValue && !body.Reported)
        {
            var dragger = MiscUtils.PlayerById(rec.DraggedByPlayerId.Value);
            if (dragger != null && dragger.Data?.Role is TownOfUs.Roles.Impostor.UndertakerRole)
            {
                TownOfUs.Roles.Impostor.UndertakerRole.RpcStartDragging(dragger, bodyId);
            }
        }
    }

    public static DeadBody? FindDeadBodyIncludingInactive(byte bodyId)
    {
        try
        {
            for (var sceneIdx = 0; sceneIdx < UnityEngine.SceneManagement.SceneManager.sceneCount; sceneIdx++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(sceneIdx);
                if (!scene.isLoaded)
                {
                    continue;
                }

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var bodies = root.GetComponentsInChildren<DeadBody>(true);
                    foreach (var body in bodies)
                    {
                        if (body != null && body.ParentId == bodyId)
                        {
                            return body;
                        }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    public static List<DeadBody> FindAllDeadBodiesIncludingInactive()
    {
        var results = new List<DeadBody>(16);
        try
        {
            for (var sceneIdx = 0; sceneIdx < UnityEngine.SceneManagement.SceneManager.sceneCount; sceneIdx++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(sceneIdx);
                if (!scene.isLoaded)
                {
                    continue;
                }

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    if (root == null)
                    {
                        continue;
                    }

                    var bodies = root.GetComponentsInChildren<DeadBody>(true);
                    foreach (var body in bodies)
                    {
                        if (body != null && body.gameObject != null)
                        {
                            results.Add(body);
                        }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        return results;
    }

    public static void SeedCleanedBodiesFromHiddenBodies()
    {
        var seeded = 0;
        var bodies = FindAllDeadBodiesIncludingInactive();
        if (bodies.Count == 0)
        {
            return;
        }

        foreach (var body in bodies)
        {
            if (body == null || body.gameObject == null)
            {
                continue;
            }

            if (body.gameObject.activeSelf)
            {
                continue;
            }

            var id = body.ParentId;
            if (CleanedBodies.ContainsKey(id))
            {
                continue;
            }

            CleanedBodies[id] = new CleanedBodyRecord(
                id,
                body.transform.position,
                DateTime.UtcNow,
                Time.time,
                body)
            {
                Restored = false,
                RestoredThisRewind = false,
                Source = CleanedBodySource.Unknown
            };

            seeded++;
            LogBodyRestore($"SeedCleanedBodiesFromHiddenBodies: seeded body={id} pos={body.transform.position} timeSeconds={Time.time:0.000}");
        }

        if (seeded > 0)
        {
            LogBodyRestore($"SeedCleanedBodiesFromHiddenBodies: seeded={seeded} (hiddenBodiesFound={seeded}, cleanedBodiesTotalNow={CleanedBodies.Count})");
        }
    }

    public static System.Collections.IEnumerator CoHideBodyForTimeLord(DeadBody body)
    {
        if (body == null)
        {
            yield break;
        }

        var renderer = body.bodyRenderers[^1];
        yield return MiscUtils.PerformTimedAction(1f, t => renderer.color = renderer.color.SetAlpha(1 - t));

        if (CleanedBodies.TryGetValue(body.ParentId, out var rec) && rec != null)
        {
            var tweakOpt = OptionGroupSingleton<VanillaTweakOptions>.Instance;
            if (tweakOpt.HidePetsOnBodyRemove.Value && (PetVisiblity)tweakOpt.ShowPetsMode.Value is PetVisiblity.AlwaysVisible)
            {
                var player = MiscUtils.PlayerById(body.ParentId);
                if (player != null && !player.AmOwner && player.CurrentOutfit.PetId != "")
                {
                    rec.OriginalPetId = player.CurrentOutfit.PetId;
                    rec.PetWasRemoved = true;
                    MiscUtils.RemovePet(player);
                    BodyLogger?.LogError($"[CoHideBodyForTimeLord] Removed pet '{rec.OriginalPetId}' from player {body.ParentId}");
                }
            }
        }

        if (CleanedBodies.TryGetValue(body.ParentId, out var rec2) && rec2 != null && rec2.Restored)
        {
            foreach (var r in body.bodyRenderers)
            {
                if (r != null)
                {
                    r.color = r.color.SetAlpha(1f);
                }
            }
            rec2.Restored = false;
            yield break;
        }

        body.gameObject.SetActive(false);
    }


    private static System.Collections.IEnumerator CoRefreshPetState(PlayerControl player)
    {
        yield return null;

        if (player != null && !player.AmOwner && player.cosmetics.CurrentPet != null)
        {
            var petId = player.CurrentOutfit.PetId;
            if (!string.IsNullOrEmpty(petId))
            {
                player.SetPet(petId);
            }
        }
    }

    public static void PruneCleanedBodies(float maxAgeSeconds)
    {
        if (CleanedBodies.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var cutoff = now - TimeSpan.FromSeconds(Math.Max(0.1f, maxAgeSeconds));
        var keys = CleanedBodies.Keys.ToList();
        foreach (var k in keys)
        {
            if (!CleanedBodies.TryGetValue(k, out var rec) || rec == null)
            {
                CleanedBodies.Remove(k);
                continue;
            }

            if (rec.TimeUtc < cutoff)
            {
                CleanedBodies.Remove(k);
                continue;
            }

            if (rec.Body == null || rec.Body.gameObject == null)
            {
                CleanedBodies.Remove(k);
            }
        }
    }

    public static void Clear()
    {
        CleanedBodies.Clear();
    }

    public static bool HasCleanedBodies()
    {
        return CleanedBodies.Count > 0;
    }

    public static int GetCleanedBodyCount()
    {
        return CleanedBodies.Count;
    }

    public static void ResetRestoredThisRewind()
    {
        foreach (var rec in CleanedBodies.Values)
        {
            if (rec != null)
            {
                rec.RestoredThisRewind = false;
            }
        }
    }

    public static IEnumerable<CleanedBodyRecord> GetCleanedBodiesForScheduling(float cutoffTime)
    {
        return CleanedBodies.Values.Where(x => x.TimeSeconds >= cutoffTime);
    }

    public static IEnumerable<CleanedBodyRecord> GetCleanedBodiesRestoredThisRewind()
    {
        return CleanedBodies.Values.Where(x => x != null && x.RestoredThisRewind);
    }
}