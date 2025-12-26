using BepInEx.Logging;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Reflection;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules;

public static class TimeLordRewindSystem
{
    public enum CleanedBodySource : byte
    {
        Unknown = 0,
        Rotting = 1,
        Janitor = 2,
    }

    private enum SpecialAnim : byte
    {
        None = 0,
        Ladder = 1,
        Zipline = 2,
        Platform = 3,
        Vent = 4,
    }

    private static readonly MethodInfo? NetTransformInvisibleAnimMethod =
        typeof(CustomNetworkTransform).GetMethod("IsInMiddleOfAnimationThatMakesPlayerInvisible",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [Flags]
    private enum SnapshotFlags : byte
    {
        None = 0,
        InVent = 1 << 0,
        WalkingToVent = 1 << 1,
        InMovingPlat = 1 << 2,
        InvisibleAnim = 1 << 3,
        InMinigame = 1 << 4,
    }

    private readonly struct Snapshot
    {
        public readonly float Time;
        public readonly Vector2 Pos;
        public readonly SpecialAnim Anim;
        public readonly SnapshotFlags Flags;
        public readonly int VentId;

        public Snapshot(float time, Vector2 pos, SpecialAnim anim, SnapshotFlags flags, int ventId)
        {
            Time = time;
            Pos = pos;
            Anim = anim;
            Flags = flags;
            VentId = ventId;
        }
    }

    private readonly struct TaskStepSnapshot
    {
        public readonly byte[] Steps;

        public TaskStepSnapshot(byte[] steps)
        {
            Steps = steps;
        }
    }

    private sealed class CircularBuffer
    {
        private Snapshot[] _items;
        private int _start;
        private int _count;

        public int Count => _count;

        public CircularBuffer(int capacity)
        {
            _items = new Snapshot[Math.Max(1, capacity)];
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity <= _items.Length)
            {
                return;
            }

            var newArr = new Snapshot[capacity];
            for (var i = 0; i < _count; i++)
            {
                newArr[i] = _items[(_start + i) % _items.Length];
            }

            _items = newArr;
            _start = 0;
        }

        public void Clear()
        {
            _start = 0;
            _count = 0;
        }

        public void Add(Snapshot item)
        {
            if (_count < _items.Length)
            {
                _items[(_start + _count) % _items.Length] = item;
                _count++;
                return;
            }

            _items[_start] = item;
            _start = (_start + 1) % _items.Length;
        }

        public bool TryPopLast(out Snapshot snapshot)
        {
            if (_count <= 0)
            {
                snapshot = default;
                return false;
            }

            var idx = (_start + _count - 1) % _items.Length;
            snapshot = _items[idx];
            _count--;
            return true;
        }

        public bool TryPeekLast(out Snapshot snapshot)
        {
            if (_count <= 0)
            {
                snapshot = default;
                return false;
            }

            var idx = (_start + _count - 1) % _items.Length;
            snapshot = _items[idx];
            return true;
        }

        public int CountNewerThan(float cutoffTime)
        {
            if (_count <= 0)
            {
                return 0;
            }

            var c = 0;
            for (var i = 0; i < _count; i++)
            {
                var idx = (_start + i) % _items.Length;
                if (_items[idx].Time >= cutoffTime)
                {
                    c++;
                }
            }

            return c;
        }

        public void RemoveOlderThan(float cutoffTime)
        {
            if (_count <= 0)
            {
                return;
            }

            var removed = 0;
            while (_count > 0)
            {
                var idx = _start % _items.Length;
                if (_items[idx].Time >= cutoffTime)
                {
                    break;
                }

                _start = (_start + 1) % _items.Length;
                _count--;
                removed++;
            }
        }
    }

    private sealed class BodyPosBuffer
    {
        private Vector2[] _items;
        private int _start;
        private int _count;

        public int Count => _count;

        public BodyPosBuffer(int capacity)
        {
            _items = new Vector2[Math.Max(1, capacity)];
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity <= _items.Length)
            {
                return;
            }

            var next = new Vector2[capacity];
            for (var i = 0; i < _count; i++)
            {
                next[i] = _items[(_start + i) % _items.Length];
            }

            _items = next;
            _start = 0;
        }

        public void Clear()
        {
            _start = 0;
            _count = 0;
        }

        public void Add(Vector2 pos)
        {
            if (_count < _items.Length)
            {
                _items[(_start + _count) % _items.Length] = pos;
                _count++;
                return;
            }

            _items[_start] = pos;
            _start = (_start + 1) % _items.Length;
        }

        public bool TryGetOldest(out Vector2 pos)
        {
            if (_count <= 0)
            {
                pos = default;
                return false;
            }

            pos = _items[_start];
            return true;
        }
    }

    private sealed class TaskStepBuffer
    {
        private byte[][] _steps; private int _start;
        private int _count;

        public TaskStepBuffer(int capacity, int maxTasks)
        {
            capacity = Math.Max(1, capacity);
            _steps = new byte[capacity][];
            for (var i = 0; i < capacity; i++)
            {
                _steps[i] = new byte[Math.Max(1, maxTasks)];
            }
        }

        public void EnsureCapacity(int capacity, int maxTasks)
        {
            capacity = Math.Max(1, capacity);
            maxTasks = Math.Max(1, maxTasks);

            if (capacity <= _steps.Length && maxTasks <= _steps[0].Length)
            {
                return;
            }

            var newSteps = new byte[Math.Max(capacity, _steps.Length)][];
            for (var i = 0; i < newSteps.Length; i++)
            {
                newSteps[i] = new byte[maxTasks];
            }

            for (var i = 0; i < _count; i++)
            {
                var oldIdx = (_start + i) % _steps.Length;
                Array.Copy(_steps[oldIdx], 0, newSteps[i], 0, Math.Min(_steps[oldIdx].Length, newSteps[i].Length));
            }

            _steps = newSteps;
            _start = 0;
        }

        public void Clear()
        {
            _start = 0;
            _count = 0;
        }

        public void Add(byte[] steps, int taskCount)
        {
            if (_steps.Length == 0)
            {
                return;
            }

            var writeIdx = (_start + _count) % _steps.Length;
            if (_count >= _steps.Length)
            {
                writeIdx = _start;
                _start = (_start + 1) % _steps.Length;
            }
            else
            {
                _count++;
            }

            Array.Clear(_steps[writeIdx], 0, _steps[writeIdx].Length);
            Array.Copy(steps, 0, _steps[writeIdx], 0, Math.Min(taskCount, _steps[writeIdx].Length));
        }

        public void TryPopLast(out TaskStepSnapshot snapshot)
        {
            if (_count <= 0)
            {
                snapshot = default;
                return;
            }

            var idx = (_start + _count - 1) % _steps.Length;
            snapshot = new TaskStepSnapshot(_steps[idx]);
            _count--;
        }

        public void RemoveCount(int countToRemove)
        {
            if (countToRemove <= 0 || _count <= 0)
            {
                return;
            }

            var removed = Math.Min(countToRemove, _count);
            _start = (_start + removed) % _steps.Length;
            _count -= removed;
        }
    }

    private static readonly CircularBuffer Buffer = new(1024);
    private static readonly TaskStepBuffer TaskBuffer = new(1024, 24);
    private static uint[] _trackedTaskIds = Array.Empty<uint>();
    private static int _trackedTaskCount;

    private sealed class SpecialClipSet
    {
        public AnimationClip? LadderAny { get; set; }
        public AnimationClip? LadderUp { get; set; }
        public AnimationClip? LadderDown { get; set; }
    }

    private static readonly Dictionary<int, SpecialClipSet> SpecialClipsByGroupHash = new();

    private static bool _colliderWasEnabled;
    private static Vector2 _finalSnapPos;
    private static bool _hasFinalSnapPos;


    private sealed class ScheduledRevive
    {
        public byte VictimId { get; }
        public float KillAgeSeconds { get; }
        public bool Done { get; set; }

        public ScheduledRevive(byte victimId, float killAgeSeconds)
        {
            VictimId = victimId;
            KillAgeSeconds = killAgeSeconds;
            Done = false;
        }
    }

    private sealed class ScheduledBodyRestore
    {
        public byte BodyId { get; }
        public float TriggerAtSeconds { get; }
        public bool Done { get; set; }

        public ScheduledBodyRestore(byte bodyId, float triggerAtSeconds)
        {
            BodyId = bodyId;
            TriggerAtSeconds = triggerAtSeconds;
            Done = false;
        }
    }

    private static float _rewindStartTime;
    private static float _rewindHistoryCutoffTime;
    private static int _lastKnownVentId = -1;

    private static Vector2 _lastRecordedPos;
    private static bool _hasLastRecordedPos;
    private static List<ScheduledRevive>? _hostRevives;
    private static List<ScheduledBodyRestore>? _localBodyRestores;
    private static float _popsPerTick;
    private static float _popAccumulator;
    private static int _popsRemaining;

    private sealed class ScheduledBodyPos
    {
        public byte BodyId { get; }
        public Vector2 Position { get; }
        public float TriggerAtSeconds { get; }
        public bool Done { get; set; }

        public ScheduledBodyPos(byte bodyId, Vector2 position, float triggerAtSeconds)
        {
            BodyId = bodyId;
            Position = position;
            TriggerAtSeconds = triggerAtSeconds;
            Done = false;
        }
    }

    private static readonly Dictionary<byte, BodyPosBuffer> HostBodyPosHistory = new();
    private static List<ScheduledBodyPos>? _hostBodyPlacements;

    private readonly struct HostTaskCompletion
    {
        public readonly byte PlayerId;
        public readonly uint TaskId;
        public readonly DateTime TimeUtc;

        public HostTaskCompletion(byte playerId, uint taskId, DateTime timeUtc)
        {
            PlayerId = playerId;
            TaskId = taskId;
            TimeUtc = timeUtc;
        }
    }

    private sealed class ScheduledTaskUndo
    {
        public byte PlayerId { get; }
        public uint TaskId { get; }
        public float TriggerAtSeconds { get; }
        public bool Done { get; set; }

        public ScheduledTaskUndo(byte playerId, uint taskId, float triggerAtSeconds)
        {
            PlayerId = playerId;
            TaskId = taskId;
            TriggerAtSeconds = triggerAtSeconds;
            Done = false;
        }
    }

    private static readonly List<HostTaskCompletion> HostTaskCompletions = new(64);
    private static List<ScheduledTaskUndo>? _hostTaskUndos;

    private sealed class CleanedBodyRecord
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
        }
    }

    private static readonly Dictionary<byte, CleanedBodyRecord> CleanedBodies = new();

    private static bool _cachedHasTimeLord;
    private static float _nextHasTimeLordCheckTime;

    public static bool IsRewinding { get; private set; }
    public static byte SourceTimeLordId { get; private set; }
    public static float RewindEndTime { get; private set; }
    public static float RewindDuration { get; private set; }

    internal static readonly ManualLogSource BodyLogger =
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

    private static List<DeadBody> FindAllDeadBodiesIncludingInactive()
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

    private static void SeedCleanedBodiesFromHiddenBodies()
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

        return;
    }

    public static void Reset()
    {
        IsRewinding = false;
        SourceTimeLordId = byte.MaxValue;
        RewindEndTime = 0f;
        RewindDuration = 0f;

        _colliderWasEnabled = false;
        _finalSnapPos = default;
        _hasFinalSnapPos = false;
        _rewindStartTime = 0f;
        _rewindHistoryCutoffTime = 0f;
        _lastKnownVentId = -1;
        _hostRevives = null;
        _localBodyRestores = null;
        _hostBodyPlacements = null;
        HostBodyPosHistory.Clear();
        _popsPerTick = 0f;
        _popAccumulator = 0f;
        _popsRemaining = 0;
        _hostTaskUndos = null;
        _cachedHasTimeLord = false;
        _nextHasTimeLordCheckTime = 0f;
        Buffer.Clear();
        TaskBuffer.Clear();
        _lastRecordedPos = default;
        _hasLastRecordedPos = false;
        _trackedTaskIds = Array.Empty<uint>();
        _trackedTaskCount = 0;
        _lastRewindAnim = SpecialAnim.None;
        _finalSnapTime = 0f;

        if (TutorialManager.InstanceExists)
        {
            PruneCleanedBodies(maxAgeSeconds: 120f);
        }
        else
        {
            CleanedBodies.Clear();
        }
    }

    public static bool MatchHasTimeLord()
    {
        var now = Time.time;
        if (now < _nextHasTimeLordCheckTime)
        {
            return _cachedHasTimeLord;
        }

        _nextHasTimeLordCheckTime = now + 1.0f;
        _cachedHasTimeLord = false;

        try
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p?.Data?.Role is TimeLordRole || (p != null && p.HasModifier<TestTimeLordModifier>()))
                {
                    _cachedHasTimeLord = true;
                    break;
                }
            }
        }
        catch
        {
            _cachedHasTimeLord = false;
        }

        return _cachedHasTimeLord;
    }

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

    private static int TryGetVentIdFromPlayerState(PlayerControl lp, PlayerPhysics? physics)
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
                var v = f.DeclaringType == typeof(PlayerPhysics)
                    ? (physics != null ? f.GetValue(physics) as Vent : null)
                    : f.GetValue(lp) as Vent;

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

    public static void ClearHostTaskHistory()
    {
        HostTaskCompletions.Clear();
    }

    public static void RecordHostTaskCompletion(PlayerControl player, PlayerTask task)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!MatchHasTimeLord())
        {
            return;
        }

        if (player == null || player.Data == null || player.Data.Disconnected)
        {
            return;
        }

        if (PlayerTask.TaskIsEmergency(task) || task.TryCast<ImportantTextTask>() != null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        HostTaskCompletions.Add(new HostTaskCompletion(player.PlayerId, task.Id, now));

        var cutoff = now - TimeSpan.FromSeconds(120);
        for (var i = HostTaskCompletions.Count - 1; i >= 0; i--)
        {
            if (HostTaskCompletions[i].TimeUtc < cutoff)
            {
                HostTaskCompletions.RemoveAt(i);
            }
        }
    }

    public static void ConfigureHostTaskUndos(List<(byte PlayerId, uint TaskId, float TriggerAtSeconds)>? schedule)
    {
        if (schedule == null || schedule.Count == 0)
        {
            _hostTaskUndos = null;
            return;
        }

        _hostTaskUndos = new List<ScheduledTaskUndo>(schedule.Count);
        foreach (var (playerId, taskId, triggerAt) in schedule)
        {
            _hostTaskUndos.Add(new ScheduledTaskUndo(playerId, taskId, triggerAt));
        }
    }

    public static void ConfigureHostTaskUndosFromHistory(float durationSeconds, float historySeconds)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            _hostTaskUndos = null;
            return;
        }

        var now = DateTime.UtcNow;
        var cutoff = now - TimeSpan.FromSeconds(historySeconds);

        var schedule = HostTaskCompletions
    .Where(x => x.TimeUtc >= cutoff)
    .GroupBy(x => (x.PlayerId, x.TaskId))
    .Select(g => g.OrderByDescending(v => v.TimeUtc).First())
    .Select(x =>
    {
        var age = (float)(now - x.TimeUtc).TotalSeconds;
        var triggerAt = durationSeconds * (age / historySeconds);
        return (x.PlayerId, x.TaskId, TriggerAtSeconds: triggerAt);
    })
    .ToList();

        ConfigureHostTaskUndos(schedule);
    }

    public static void RecordLocalSnapshot(PlayerPhysics? physics)
    {
        if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data == null)
        {
            return;
        }

        if (IsRewinding)
        {
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (IntroCutscene.Instance != null)
        {
            return;
        }

        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started &&
            AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
        {
            return;
        }

        if (!MatchHasTimeLord())
        {
            return;
        }

        var lp = PlayerControl.LocalPlayer;

        var history = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 60f);
        var dt = Mathf.Max(Time.fixedDeltaTime, 0.001f);
        var samplesNeeded = (int)Math.Ceiling(history / dt) + 5;
        Buffer.EnsureCapacity(Math.Clamp(samplesNeeded, 256, 4096));

        const float retentionBuffer = 7.5f;
        var retentionCutoff = Time.time - (history + retentionBuffer);
        var countBefore = Buffer.Count;
        Buffer.RemoveOlderThan(retentionCutoff);
        var removedCount = countBefore - Buffer.Count;
        
        if (removedCount > 0)
        {
            TaskBuffer.RemoveCount(removedCount);
        }

        var anim = SpecialAnim.None;
        if (lp.onLadder)
        {
            anim = SpecialAnim.Ladder;
        }

        var flags = SnapshotFlags.None;
        var ventId = -1;
        if (lp.inVent)
        {
            flags |= SnapshotFlags.InVent;
        }
        if (lp.walkingToVent)
        {
            flags |= SnapshotFlags.WalkingToVent;
        }
        if ((flags & (SnapshotFlags.InVent | SnapshotFlags.WalkingToVent)) != 0)
        {
            ventId = TryGetVentIdFromPlayerState(lp, physics);
            if (ventId < 0)
            {
                ventId = _lastKnownVentId;
            }
            else
            {
                _lastKnownVentId = ventId;
            }
        }
        if (lp.inMovingPlat)
        {
            flags |= SnapshotFlags.InMovingPlat;
        }
        if (!lp.inVent && IsInInvisibleAnimation(lp))
        {
            flags |= SnapshotFlags.InvisibleAnim;
        }

        var inMinigame = Minigame.Instance != null || SpawnInMinigame.Instance != null;
        if (inMinigame)
        {
            flags |= SnapshotFlags.InMinigame;
        }

        var pos = (lp.inMovingPlat || lp.walkingToVent) ? (Vector2)lp.transform.position :
(physics?.body != null ? physics.body.position : (Vector2)lp.transform.position);

        if (inMinigame && _hasLastRecordedPos)
        {
            pos = _lastRecordedPos;
        }

        if (!IsValidSnapshotPos(lp, pos))
        {
            return;
        }

        Buffer.Add(new Snapshot(Time.time, pos, anim, flags, ventId));
        _lastRecordedPos = pos;
        _hasLastRecordedPos = true;

        if (OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind)
        {
            RecordLocalTaskSteps(lp, samplesNeeded);
        }
    }

    private static void RecordLocalTaskSteps(PlayerControl lp, int samplesNeeded)
    {
        var tasks = lp.myTasks.ToArray()
.Where(t => t != null && t.TryCast<NormalPlayerTask>() != null && !PlayerTask.TaskIsEmergency(t) &&
        t.TryCast<ImportantTextTask>() == null)
.ToList();

        var taskCount = Math.Min(tasks.Count, 24);
        var idsChanged = _trackedTaskCount != taskCount;
        if (!idsChanged && taskCount > 0)
        {
            for (var i = 0; i < taskCount; i++)
            {
                var id = tasks[i].Id;
                if (i >= _trackedTaskIds.Length || _trackedTaskIds[i] != id)
                {
                    idsChanged = true;
                    break;
                }
            }
        }

        if (idsChanged)
        {
            _trackedTaskIds = new uint[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                _trackedTaskIds[i] = tasks[i].Id;
            }

            _trackedTaskCount = taskCount;
            TaskBuffer.Clear();
        }

        TaskBuffer.EnsureCapacity(Math.Clamp(samplesNeeded, 256, 4096), Math.Max(taskCount, 1));

        var steps = new byte[Math.Max(taskCount, 1)];
        for (var i = 0; i < taskCount; i++)
        {
            var npt = tasks[i].Cast<NormalPlayerTask>();
            steps[i] = (byte)Math.Clamp(npt.taskStep, 0, 255);
        }

        TaskBuffer.Add(steps, taskCount);
    }

    public static void ConfigureHostRevives(List<(byte VictimId, float KillAgeSeconds)>? revives)
    {
        if (revives == null || revives.Count == 0)
        {
            _hostRevives = null;
            return;
        }

        _hostRevives = new List<ScheduledRevive>(revives.Count);
        foreach (var (victimId, killAge) in revives)
        {
            _hostRevives.Add(new ScheduledRevive(victimId, killAge));
        }
    }

    public static void StartRewind(byte sourceTimeLordId, float duration)
    {
        SourceTimeLordId = sourceTimeLordId;
        RewindDuration = duration;
        RewindEndTime = Time.time + duration;
        IsRewinding = true;
        _rewindStartTime = Time.time;
        _hasFinalSnapPos = false;
        _popAccumulator = 0f;
        _localBodyRestores = null;

        foreach (var rec in CleanedBodies.Values)
        {
            if (rec != null)
            {
                rec.RestoredThisRewind = false;
            }
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var history = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 60f);
        var dt = Mathf.Max(Time.fixedDeltaTime, 0.001f);
        var totalTicks = Math.Max(1, (int)Math.Round(duration / dt));
        _rewindHistoryCutoffTime = Time.time - history;
        _popsRemaining = Buffer.CountNewerThan(_rewindHistoryCutoffTime);
        _popsPerTick = _popsRemaining / (float)totalTicks;

        if (Buffer.Count == 0)
        {
            IsRewinding = false;
            RewindEndTime = 0f;
            RewindDuration = 0f;
            return;
        }

        if (Minigame.Instance != null)
        {
            try
            {
                Minigame.Instance.Close();
                Minigame.Instance.Close();
            }
            catch
            {
               // ignored
            }
        }

        if (MapBehaviour.Instance != null)
        {
            try
            {
                MapBehaviour.Instance.Close();
                MapBehaviour.Instance.Close();
            }
            catch
            {
               // ignored
            }
        }

        // Exempt ghosts and ghost roles from Time Lord effects
        var isGhost = PlayerControl.LocalPlayer.Data.IsDead || 
                      PlayerControl.LocalPlayer.Data.Role is IGhostRole;
        
        if (!isGhost)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
            PlayerControl.LocalPlayer.moveable = false;
            if (PlayerControl.LocalPlayer.MyPhysics?.body != null)
            {
                PlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
            }

            if (PlayerControl.LocalPlayer.onLadder)
            {
                try
                {
                    PlayerControl.LocalPlayer.MyPhysics?.StopAllCoroutines();
                }
                catch
                {
                }
                PlayerControl.LocalPlayer.onLadder = false;
            }

            if (PlayerControl.LocalPlayer.Collider != null)
            {
                _colliderWasEnabled = PlayerControl.LocalPlayer.Collider.enabled;
                PlayerControl.LocalPlayer.Collider.enabled = false;
            }
        }

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.TimeLord, duration, 0.25f));

        ConfigureLocalBodyRestores(duration, history);

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            ConfigureHostBodyPlacements(duration);
        }
    }

    public static void RecordHostBodyPositions()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (IsRewinding || !MatchHasTimeLord())
        {
            return;
        }

        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started &&
            AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
        {
            return;
        }

        var history = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 60f);
        var dt = Mathf.Max(Time.fixedDeltaTime, 0.001f);
        var samplesNeeded = (int)Math.Ceiling(history / dt) + 5;
        var cap = Math.Clamp(samplesNeeded, 64, 4096);

        var seen = new HashSet<byte>();
        foreach (var body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body == null)
            {
                continue;
            }

            var id = body.ParentId;
            seen.Add(id);

            if (!HostBodyPosHistory.TryGetValue(id, out var buf))
            {
                buf = new BodyPosBuffer(cap);
                HostBodyPosHistory[id] = buf;
            }

            buf.EnsureCapacity(cap);
            buf.Add(body.transform.position);
        }

        var keys = HostBodyPosHistory.Keys.ToList();
        foreach (var k in keys)
        {
            if (!seen.Contains(k))
            {
                HostBodyPosHistory.Remove(k);
            }
        }
    }

    private static void ConfigureHostBodyPlacements(float durationSeconds)
    {
        if (!MatchHasTimeLord() || HostBodyPosHistory.Count == 0)
        {
            _hostBodyPlacements = null;
            return;
        }

        var list = new List<ScheduledBodyPos>(HostBodyPosHistory.Count);
        foreach (var (bodyId, buf) in HostBodyPosHistory)
        {
            if (buf == null || !buf.TryGetOldest(out var pos))
            {
                continue;
            }

            var triggerAt = Math.Max(0.05f, durationSeconds - 0.05f);
            list.Add(new ScheduledBodyPos(bodyId, pos, triggerAt));
        }

        _hostBodyPlacements = list.Count > 0 ? list : null;
    }

    private static void ConfigureLocalBodyRestores(float durationSeconds, float historySeconds)
    {
        if (!OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind)
        {
            _localBodyRestores = null;
            LogBodyRestore(
                $"ConfigureLocalBodyRestores: skipped (option={OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind}, cleanedBodies={CleanedBodies.Count})");
            return;
        }

        if (CleanedBodies.Count == 0)
        {
            SeedCleanedBodiesFromHiddenBodies();
        }

        if (CleanedBodies.Count == 0)
        {
            _localBodyRestores = null;
            LogBodyRestore("ConfigureLocalBodyRestores: no cleaned bodies to schedule after seeding attempt");
            return;
        }

        var nowTime = Time.time;
        var cutoffTime = nowTime - historySeconds;

        var schedule = CleanedBodies.Values
                                    .Where(x => x.TimeSeconds >= cutoffTime)
            .Select(x =>
            {
                var age = Math.Max(0f, nowTime - x.TimeSeconds);
                var triggerAt = durationSeconds * (age / historySeconds);
                LogBodyRestore(
                    $"ConfigureLocalBodyRestores: body={x.BodyId} age={age:0.000}s history={historySeconds:0.000}s duration={durationSeconds:0.000}s triggerAt={triggerAt:0.000}s active={(x.Body != null && x.Body.gameObject != null ? x.Body.gameObject.activeSelf : (bool?)null)} source={x.Source}");
                return new ScheduledBodyRestore(x.BodyId, triggerAt);
            })
            .ToList();

        _localBodyRestores = schedule.Count > 0 ? schedule : null;

        LogBodyRestore(
            $"ConfigureLocalBodyRestores: scheduled={schedule.Count} (cleanedBodiesTotal={CleanedBodies.Count}, cutoffTime={cutoffTime:0.000}, nowTime={nowTime:0.000})");
    }

    private static Vent? GetVentById(int id)
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

    private static void ApplyVentSnapshotState(PlayerControl lp, Snapshot snap)
    {
        var wantInVent = (snap.Flags & SnapshotFlags.InVent) != 0;

        if (wantInVent)
        {
            if (snap.VentId < 0)
            {
                wantInVent = false;
            }
            else
            {
                var v = GetVentById(snap.VentId);
                if (v == null)
                {
                    wantInVent = false;
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
        lp.walkingToVent = (snap.Flags & SnapshotFlags.WalkingToVent) != 0;
    }

    public static bool TryHandleRewindPhysics(PlayerPhysics physics)
    {
        if (!IsRewinding || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
        {
            return false;
        }

        // Exempt ghosts and ghost roles from Time Lord effects - they can move freely
        var isGhost = PlayerControl.LocalPlayer.Data.IsDead || 
                      PlayerControl.LocalPlayer.Data.Role is IGhostRole;
        
        if (isGhost)
        {
            if (Time.time >= RewindEndTime)
            {
                StopRewind();
            }
            return false; // Return false to allow normal physics update
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            StopRewind();
            return true;
        }

        var lp = PlayerControl.LocalPlayer;
        if (physics == null || physics.myPlayer != lp)
        {
            return false;
        }

        var unsafeNow = lp.walkingToVent || lp.inMovingPlat || (!lp.inVent && IsInInvisibleAnimation(lp));
        if (unsafeNow)
        {
            if (Time.time >= RewindEndTime)
            {
                StopRewind();
                return true;
            }

            lp.inMovingPlat = false;
            lp.walkingToVent = false;
            try { physics.ResetAnimState(); } catch { /* ignored */ }
        }

        lp.moveable = false;

        if (Time.time >= RewindEndTime)
        {
            StopRewind();
            return true;
        }

        if (_popsRemaining <= 0 || Buffer.Count == 0)
        {
            lp.moveable = false;
            if (physics.body != null)
            {
                physics.body.velocity = Vector2.zero;
            }
            physics.SetNormalizedVelocity(Vector2.zero);

            if (_hasFinalSnapPos)
            {
                lp.transform.position = _finalSnapPos;
                if (physics.body != null)
                {
                    physics.body.position = _finalSnapPos;
                }
                lp.NetTransform?.SnapTo(_finalSnapPos);
            }

            return true;
        }

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && _hostRevives != null && _hostRevives.Count > 0)
        {
            if (!OptionGroupSingleton<TimeLordOptions>.Instance.ReviveOnRewind)
            {
                _hostRevives = null;
            }
            else
            {
                var elapsed = Time.time - _rewindStartTime;
                for (var i = 0; i < _hostRevives.Count; i++)
                {
                    var entry = _hostRevives[i];
                    if (entry.Done)
                    {
                        continue;
                    }

                    if (elapsed + 0.0001f >= entry.KillAgeSeconds)
                    {
                        entry.Done = true;

                        var victim = MiscUtils.PlayerById(entry.VictimId);
                        if (victim != null && victim.Data != null && !victim.Data.Disconnected && victim.Data.IsDead)
                        {
                            TimeLordRole.RpcRewindRevive(victim);
                        }
                    }
                }
            }
        }

        if (_localBodyRestores != null && _localBodyRestores.Count > 0)
        {
            var elapsed = Time.time - _rewindStartTime;
            for (var i = 0; i < _localBodyRestores.Count; i++)
            {
                var entry = _localBodyRestores[i];
                if (entry.Done)
                {
                    continue;
                }

                if (elapsed + 0.0001f >= entry.TriggerAtSeconds)
                {
                    entry.Done = true;
                    RestoreCleanedBody(entry.BodyId);
                }
            }
        }

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && _hostBodyPlacements != null &&
    _hostBodyPlacements.Count > 0)
        {
            var elapsed = Time.time - _rewindStartTime;
            for (var i = 0; i < _hostBodyPlacements.Count; i++)
            {
                var entry = _hostBodyPlacements[i];
                if (entry.Done)
                {
                    continue;
                }

                if (elapsed + 0.0001f >= entry.TriggerAtSeconds)
                {
                    entry.Done = true;
                    TownOfUs.Roles.Crewmate.TimeLordRole.RpcSetDeadBodyPos(PlayerControl.LocalPlayer, entry.BodyId,
                        entry.Position);
                }
            }
        }

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && _hostTaskUndos != null && _hostTaskUndos.Count > 0)
        {
            var elapsed = Time.time - _rewindStartTime;
            for (var i = 0; i < _hostTaskUndos.Count; i++)
            {
                var entry = _hostTaskUndos[i];
                if (entry.Done)
                {
                    continue;
                }

                if (elapsed + 0.0001f >= entry.TriggerAtSeconds)
                {
                    entry.Done = true;
                    TownOfUs.Roles.Crewmate.TimeLordRole.RpcUndoTask(PlayerControl.LocalPlayer, entry.PlayerId, entry.TaskId);
                }
            }
        }

        _popAccumulator += _popsPerTick;
        var popsThisTick = Mathf.FloorToInt(_popAccumulator);
        if (popsThisTick <= 0)
        {
            popsThisTick = 1;
        }
        _popAccumulator -= popsThisTick;

        if (Buffer.TryPeekLast(out var peek) && (peek.Flags & SnapshotFlags.InMinigame) != 0)
        {
            const int maxPopsPerTick = 64;
            const int minigameFastForwardMultiplier = 4;
            var fastForwardPops = Math.Min(maxPopsPerTick, popsThisTick * minigameFastForwardMultiplier);
            var maxAllowedPops = Math.Max(1, (int)(_popsRemaining * 0.8f));
            popsThisTick = Math.Min(fastForwardPops, maxAllowedPops);
        }

        popsThisTick = Math.Min(popsThisTick, _popsRemaining);
        if (popsThisTick <= 0)
        {
            StopRewind();
            return true;
        }

        Snapshot snap = default;
        TaskStepSnapshot taskSnap = default;
        var hitHistoryCutoff = false;
        for (var i = 0; i < popsThisTick; i++)
        {
            if (!Buffer.TryPopLast(out snap))
            {
                StopRewind();
                return true;
            }
            if (snap.Time < _rewindHistoryCutoffTime)
            {
                hitHistoryCutoff = true;
                break;
            }
            if (OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind && _trackedTaskCount > 0)
            {
                TaskBuffer.TryPopLast(out taskSnap);
            }
            _popsRemaining--;
        }

        if (hitHistoryCutoff)
        {
            StopRewind();
            return true;
        }


        while (!IsValidSnapshotPos(lp, snap.Pos) && Buffer.TryPopLast(out var next))
        {
            snap = next;
            if (snap.Time < _rewindHistoryCutoffTime)
            {
                StopRewind();
                return true;
            }
            _popsRemaining = Math.Max(0, _popsRemaining - 1);
        }

        if (!lp.Data.IsDead)
        {
            lp.onLadder = snap.Anim == SpecialAnim.Ladder;
        }

        ApplyVentSnapshotState(lp, snap);

        if (_lastRewindAnim == SpecialAnim.Ladder && snap.Anim != SpecialAnim.Ladder)
        {
            try
            {
                physics.ResetAnimState();
            }
            catch
            {
               // ignored
            }
        }

        _lastRewindAnim = snap.Anim;

        if (lp.inVent && Vent.currentVent != null)
        {
            var vpos = (Vector2)Vent.currentVent.transform.position;
            _finalSnapPos = vpos;
            _hasFinalSnapPos = true;
            _finalSnapFlags = snap.Flags;
            _finalSnapVentId = snap.VentId;
            _finalSnapTime = snap.Time;

            lp.transform.position = vpos;
            if (physics.body != null)
            {
                physics.body.position = vpos;
                physics.body.velocity = Vector2.zero;
            }
            lp.NetTransform?.SnapTo(vpos);
            physics.SetNormalizedVelocity(Vector2.zero);
            return true;
        }

        var curPos = physics.body != null ? physics.body.position : (Vector2)lp.transform.position;
        var delta = snap.Pos - curPos;

        _finalSnapPos = snap.Pos;
        _hasFinalSnapPos = true;
        _finalSnapFlags = snap.Flags;
        _finalSnapVentId = snap.VentId;
        _finalSnapTime = snap.Time;

        if (OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind && _trackedTaskCount > 0 && taskSnap.Steps != null)
        {
            ApplyLocalTaskSteps(PlayerControl.LocalPlayer, taskSnap);
        }

        const float idleEpsilon = 0.0005f;
        if (delta.sqrMagnitude <= idleEpsilon * idleEpsilon)
        {
            if (snap.Anim is SpecialAnim.Ladder)
            {
                ApplySpecialAnimation(physics, snap.Anim, delta);
            }
            else
            {
                physics.HandleAnimation(lp.Data.IsDead);
            }
            physics.SetNormalizedVelocity(Vector2.zero);
            if (physics.body != null)
            {
                physics.body.velocity = Vector2.zero;
            }
        }
        else
        {
            var dt = Mathf.Max(Time.fixedDeltaTime, 0.001f);
            var desiredVel = delta / dt;
            var dir = desiredVel.normalized;

            if (snap.Anim is SpecialAnim.Ladder)
            {
                ApplySpecialAnimation(physics, snap.Anim, delta);
            }
            else
            {
                physics.HandleAnimation(lp.Data.IsDead);
            }
            if (snap.Anim is SpecialAnim.Ladder)
            {
                physics.SetNormalizedVelocity(Vector2.zero);
            }
            else
            {
                physics.SetNormalizedVelocity(-dir);
            }

            if (physics.body != null)
            {
                physics.body.velocity = desiredVel;
            }
        }

        if (ModCompatibility.IsSubmerged())
        {
            ModCompatibility.ChangeFloor(lp.GetTruePosition().y > -7);
            ModCompatibility.CheckOutOfBoundsElevator(lp);
        }

        return true;

        /*if (!Buffer.TryPopLast(out var snap))
{
    StopRewind();
    return true;
}

var curPos = physics.body != null ? physics.body.position : (Vector2)lp.transform.position;
var delta = snap.Pos - curPos;
        _finalSnapPos = snap.Pos;
_hasFinalSnapPos = true;

        const float idleEpsilon = 0.0005f;
if (delta.sqrMagnitude <= idleEpsilon * idleEpsilon)
{
    physics.HandleAnimation(lp.Data.IsDead);
    physics.SetNormalizedVelocity(Vector2.zero);
    if (physics.body != null)
    {
        physics.body.velocity = Vector2.zero;
    }
}
else
{
                            var dt = Mathf.Max(Time.fixedDeltaTime, 0.001f);
    var desiredVel = delta / dt;
    var dir = desiredVel.normalized;

    physics.HandleAnimation(lp.Data.IsDead);
    physics.SetNormalizedVelocity(dir);

    if (physics.body != null)
    {
        physics.body.velocity = desiredVel;
    }
}

if (ModCompatibility.IsSubmerged())
{
    ModCompatibility.ChangeFloor(lp.GetTruePosition().y > -7);
    ModCompatibility.CheckOutOfBoundsElevator(lp);
}

return true;*/
    }

    public static void StopRewind()
    {
        IsRewinding = false;
        SourceTimeLordId = byte.MaxValue;
        RewindEndTime = 0f;
        RewindDuration = 0f;
        _hostRevives = null;
        _popsPerTick = 0f;
        _popAccumulator = 0f;
        _popsRemaining = 0;
        _hostTaskUndos = null;
        TaskBuffer.Clear();

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var lp = PlayerControl.LocalPlayer;

        AdvanceFinalSnapToSafeIfNeeded(lp);

        if (!lp.onLadder && !lp.inMovingPlat && !lp.inVent)
        {
            lp.moveable = true;
        }

        if (lp.MyPhysics?.body != null)
        {
            lp.MyPhysics.body.velocity = Vector2.zero;
        }

        PopOutOfVentIfNeeded(lp);

        var endedInVent = (_finalSnapFlags & SnapshotFlags.InVent) != 0;
        if (endedInVent)
        {
            try
            {
                if (_finalSnapVentId >= 0)
                {
                    var v = GetVentById(_finalSnapVentId);
                    if (v != null && lp.inVent)
                    {
                        try { v.SetButtons(false); } catch { /* ignored */ }
                        try { lp.MyPhysics?.RpcExitVent(v.Id); } catch { /* ignored */ }
                        try { lp.MyPhysics?.ExitAllVents(); } catch { /* ignored */ }
                    }
                }
                lp.inVent = false;
                Vent.currentVent = null;
            }
            catch
            {
               // ignored
            }
        }

        lp.onLadder = false;
        lp.inMovingPlat = false;
        lp.walkingToVent = false;
        try { lp.MyPhysics?.ResetAnimState(); } catch { /* ignored */ }

        if (_hasFinalSnapPos && !endedInVent)
        {
            lp.transform.position = _finalSnapPos;
            if (lp.MyPhysics?.body != null)
            {
                lp.MyPhysics.body.position = _finalSnapPos;
            }
            lp.NetTransform?.SnapTo(_finalSnapPos);
        }

        if (lp.Collider != null)
        {
            lp.Collider.enabled = endedInVent || _colliderWasEnabled;
            _colliderWasEnabled = false;

            if (!endedInVent)
            {
                TryUnstuckLocalPlayer(lp);
            }
        }

        if (lp.AmOwner && lp.NetTransform != null)
        {
            lp.NetTransform.RpcSnapTo(_hasFinalSnapPos ? _finalSnapPos : (Vector2)lp.transform.position);
        }

        if (ModCompatibility.IsSubmerged())
        {
            ModCompatibility.CheckOutOfBoundsElevator(lp);
        }

        ScheduleRottingRecleanAfterRewind();
    }

    private static void ScheduleRottingRecleanAfterRewind()
    {
        if (!OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind)
        {
            return;
        }

        if (CleanedBodies.Count == 0)
        {
            return;
        }

        foreach (var rec in CleanedBodies.Values.ToList())
        {
            if (rec == null ||
                !rec.RestoredThisRewind ||
                rec.Source != CleanedBodySource.Rotting ||
                rec.Body == null ||
                rec.Body.gameObject == null ||
                !rec.Body.gameObject.activeSelf)
            {
                continue;
            }

            var player = MiscUtils.PlayerById(rec.BodyId);
            if (player != null && player.HasModifier<RottingModifier>())
            {
                Coroutines.Start(RottingModifier.StartRotting(player));
            }
        }
    }

    public static void CancelRewindForMeeting()
    {
        if (!IsRewinding)
        {
            return;
        }

        IsRewinding = false;
        SourceTimeLordId = byte.MaxValue;
        RewindEndTime = 0f;
        RewindDuration = 0f;
        _rewindStartTime = 0f;
        _hostRevives = null;
        _localBodyRestores = null;
        _hostBodyPlacements = null;
        _hostTaskUndos = null;
        _popsPerTick = 0f;
        _popAccumulator = 0f;
        _popsRemaining = 0;
        _hasFinalSnapPos = false;
        _finalSnapPos = default;
        _finalSnapFlags = SnapshotFlags.None;
        _finalSnapVentId = -1;
        _lastRewindAnim = SpecialAnim.None;
        TaskBuffer.Clear();

        var lp = PlayerControl.LocalPlayer;
        if (!lp)
        {
            return;
        }

        if (lp.Collider != null)
        {
            lp.Collider.enabled = _colliderWasEnabled;
            _colliderWasEnabled = false;
        }

        if (lp.MyPhysics?.body != null)
        {
            lp.MyPhysics.body.velocity = Vector2.zero;
        }

        lp.onLadder = false;
        lp.inMovingPlat = false;
        lp.walkingToVent = false;
        try { lp.MyPhysics?.ResetAnimState(); } catch { /* ignored */ }

        try
        {
            var cur = (Vector2)lp.transform.position;
            lp.NetTransform?.SnapTo(cur);
        }
        catch
        {
            // ignored
        }

        lp.moveable = true;
    }

    private static void ApplyLocalTaskSteps(PlayerControl lp, TaskStepSnapshot snap)
    {
        if (lp.AmOwner && Minigame.Instance != null)
        {
            return;
        }

        for (var i = 0; i < _trackedTaskCount; i++)
        {
            var id = _trackedTaskIds[i];
            var target = lp.myTasks.ToArray().FirstOrDefault(t => t != null && t.Id == id && t.TryCast<NormalPlayerTask>() != null);
            if (target == null)
            {
                continue;
            }

            var npt = target.Cast<NormalPlayerTask>();
            if (npt.TaskType is TaskTypes.InspectSample or TaskTypes.FuelEngines or global::TaskTypes.ExtractFuel or TaskTypes.ChartCourse or TaskTypes.UploadData or TaskTypes.SortRecords or TaskTypes.OpenWaterways or TaskTypes.EmptyGarbage or TaskTypes.EmptyChute or TaskTypes.SubmitScan or TaskTypes.RebootWifi or TaskTypes.WaterPlants or TaskTypes.PickUpTowels or TaskTypes.DevelopPhotos)
            {
                continue;
            }

            var desiredStep = (int)snap.Steps[Math.Min(i, snap.Steps.Length - 1)];
            if (npt.taskStep != desiredStep)
            {
                npt.taskStep = desiredStep;
                npt.UpdateArrowAndLocation();
            }
        }

        if (lp.Data?.Role is SnitchRole snitch)
        {
            snitch.RecalculateTaskStage();
        }
    }

    private static void ApplySpecialAnimation(PlayerPhysics physics, SpecialAnim anim, Vector2 delta)
    {
        if (physics?.Animations?.Animator == null)
        {
            return;
        }

        var animator = physics.Animations.Animator;
        var group = physics.Animations.group;
        if (group == null)
        {
            return;
        }

        var hash = group.GetHashCode();
        if (!SpecialClipsByGroupHash.TryGetValue(hash, out var set))
        {
            set = BuildSpecialClipSet(group);
            SpecialClipsByGroupHash[hash] = set;
        }

        AnimationClip? desired = null;
        var goingUp = delta.y > 0.001f;
        var goingDown = delta.y < -0.001f;

        if (anim == SpecialAnim.Ladder)
        {
            if (goingUp)
            {
                desired = set.LadderDown ?? set.LadderAny;
            }
            else if (goingDown)
            {
                desired = set.LadderUp ?? set.LadderAny;
            }
        }
        else
        {
            return;
        }

        if (desired == null)
        {
            return;
        }

        try
        {
            var cur = animator.GetCurrentAnimation();
            if (cur != desired)
            {
                animator.Play(desired);
            }
        }
        catch
        {
            try
            {
                animator.Play(desired);
            }
            catch
            {
               // ignored
            }
        }
    }

    private static SpecialClipSet BuildSpecialClipSet(object group)
    {
        var set = new SpecialClipSet();
        var groupType = group.GetType();

        foreach (var field in groupType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.FieldType != typeof(AnimationClip))
            {
                continue;
            }

            var clip = field.GetValue(group) as AnimationClip;
            if (clip == null)
            {
                continue;
            }

            ClassifyClip(set, field.Name, clip);
        }

        foreach (var prop in groupType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length != 0 || prop.PropertyType != typeof(AnimationClip))
            {
                continue;
            }

            AnimationClip? clip = null;
            try
            {
                clip = prop.GetValue(group, null) as AnimationClip;
            }
            catch
            {
               // ignored
            }

            if (clip == null)
            {
                continue;
            }

            ClassifyClip(set, prop.Name, clip);
        }

        return set;
    }

    private static void ClassifyClip(SpecialClipSet set, string memberName, AnimationClip clip)
    {
        var memberLower = (memberName ?? string.Empty).ToLowerInvariant();
        var clipLower = (clip.name ?? string.Empty).ToLowerInvariant();

        bool AnyContains(string s) => memberLower.Contains(s) || clipLower.Contains(s);

        if (AnyContains("ladder") || AnyContains("climb"))
        {
            set.LadderAny ??= clip;
            if (AnyContains("up") || AnyContains("top"))
            {
                set.LadderUp ??= clip;
            }
            if (AnyContains("down") || AnyContains("bottom"))
            {
                set.LadderDown ??= clip;
            }
        }

    }

    private static bool IsInInvisibleAnimation(PlayerControl lp)
    {
        try
        {
            if (NetTransformInvisibleAnimMethod != null && lp.NetTransform != null)
            {
                return (bool)NetTransformInvisibleAnimMethod.Invoke(lp.NetTransform, null)!;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    private static bool IsValidSnapshotPos(PlayerControl lp, Vector2 pos)
    {
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsInfinity(pos.x) || float.IsInfinity(pos.y))
        {
            return false;
        }

        var cur = lp != null ? (Vector2)lp.transform.position : Vector2.zero;
        if (pos == Vector2.zero && (cur - pos).sqrMagnitude > 0.5f * 0.5f)
        {
            return false;
        }

        if (Mathf.Abs(pos.x) > 200f || Mathf.Abs(pos.y) > 200f)
        {
            return false;
        }

        return true;
    }

    public static void RecordBodyCleaned(DeadBody body)
    {
        RecordBodyCleaned(body, CleanedBodySource.Unknown);
    }

    public static void RecordBodyCleaned(DeadBody body, CleanedBodySource source)
    {
        if (body == null)
        {
            return;
        }

        LogBodyRestore(
            $"RecordBodyCleaned: body={body.ParentId} active={body.gameObject != null && body.gameObject.activeSelf} pos={body.transform.position} timeSeconds={Time.time:0.000} timeUtc={DateTime.UtcNow:O} source={source}");

        CleanedBodies[body.ParentId] = new CleanedBodyRecord(
            body.ParentId,
            body.transform.position,
            DateTime.UtcNow,
            Time.time,
            body)
        {
            Restored = false,
            RestoredThisRewind = false,
            Source = source
        };
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

    private static void PruneCleanedBodies(float maxAgeSeconds)
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

    private static void RestoreCleanedBody(byte bodyId)
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
        if (IsRewinding)
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

    private static SnapshotFlags _finalSnapFlags;
    private static SpecialAnim _lastRewindAnim = SpecialAnim.None;
    private static int _finalSnapVentId = -1;
    private static float _finalSnapTime;

    private static void AdvanceFinalSnapToSafeIfNeeded(PlayerControl lp)
    {
        var unsafeNow = lp.walkingToVent || lp.inMovingPlat || (!lp.inVent && IsInInvisibleAnimation(lp));
        var unsafeLanding = (_finalSnapFlags & (SnapshotFlags.WalkingToVent | SnapshotFlags.InMovingPlat | SnapshotFlags.InvisibleAnim)) != 0;
        if (!unsafeNow && !unsafeLanding)
        {
            return;
        }

        while (Buffer.TryPopLast(out var snap))
        {
            var snapUnsafe = (snap.Flags & (SnapshotFlags.WalkingToVent | SnapshotFlags.InMovingPlat | SnapshotFlags.InvisibleAnim)) != 0;
            if (snapUnsafe)
            {
                continue;
            }

            _finalSnapPos = snap.Pos;
            _finalSnapFlags = snap.Flags;
            _finalSnapVentId = snap.VentId;
            _hasFinalSnapPos = true;
            _finalSnapTime = snap.Time;
            break;
        }
    }

    private static void PopOutOfVentIfNeeded(PlayerControl lp)
    {
        if (!lp)
        {
            return;
        }

        if ((_finalSnapFlags & SnapshotFlags.InVent) != 0)
        {
            return;
        }

        var shouldPop = lp.inVent || lp.walkingToVent || (_finalSnapFlags & SnapshotFlags.WalkingToVent) != 0;
        if (!shouldPop)
        {
            return;
        }

        try
        {
            if (_finalSnapVentId >= 0 && ShipStatus.Instance != null && ShipStatus.Instance.AllVents != null)
            {
                var v = ShipStatus.Instance.AllVents.FirstOrDefault(x => x != null && x.Id == _finalSnapVentId);
                if (v != null)
                {
                    Vent.currentVent = v;
                    lp.inVent = true;
                    lp.MyPhysics?.RpcExitVent(v.Id);
                }
            }

            lp.MyPhysics?.ExitAllVents();
        }
        catch
        {
            // ignored
        }

        lp.walkingToVent = false;
        lp.inVent = false;
        Vent.currentVent = null;

        try { lp.MyPhysics?.ResetAnimState(); } catch { /* ignored */ }
    }


    public static void UndoTask(byte targetPlayerId, uint taskId)
    {
        var player = MiscUtils.PlayerById(targetPlayerId);
        if (player == null || player.Data == null || player.Data.Disconnected)
        {
            return;
        }

        if (player.AmOwner && Minigame.Instance != null)
        {
            try
            {
                Minigame.Instance.Close();
            }
            catch
            {
               // ignored
            }
        }

        var taskInfo = player.Data.FindTaskById(taskId);
        if (taskInfo != null)
        {
            taskInfo.Complete = false;
        }

        foreach (var t in player.myTasks.ToArray())
        {
            if (t == null || t.Id != taskId)
            {
                continue;
            }

            var normal = t.TryCast<NormalPlayerTask>();
            if (normal != null)
            {
                var n = normal.Cast<NormalPlayerTask>();
                n.taskStep = n.TaskType == TaskTypes.UploadData ? 1 : 0;
                n.Initialize();
                n.UpdateArrowAndLocation();
            }

            break;
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p?.Data?.Role is SnitchRole snitch)
            {
                snitch.RecalculateTaskStage();
            }
        }
    }

    private static void TryUnstuckLocalPlayer(PlayerControl lp)
    {
        if (lp.Collider == null)
        {
            return;
        }

        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = Constants.ShipAndAllObjectsMask,
            useTriggers = false
        };

        var results = new Collider2D[8];
        var overlapCount = lp.Collider.OverlapCollider(filter, results);
        if (overlapCount <= 0)
        {
            return;
        }

        var basePos = lp.transform.position;
        var steps = new[]
{
            new Vector2(0f, 0f),
            new Vector2(0.05f, 0f), new Vector2(-0.05f, 0f), new Vector2(0f, 0.05f), new Vector2(0f, -0.05f),
            new Vector2(0.10f, 0f), new Vector2(-0.10f, 0f), new Vector2(0f, 0.10f), new Vector2(0f, -0.10f),
            new Vector2(0.10f, 0.10f), new Vector2(-0.10f, 0.10f), new Vector2(0.10f, -0.10f), new Vector2(-0.10f, -0.10f),
            new Vector2(0.20f, 0f), new Vector2(-0.20f, 0f), new Vector2(0f, 0.20f), new Vector2(0f, -0.20f),
            new Vector2(0.20f, 0.20f), new Vector2(-0.20f, 0.20f), new Vector2(0.20f, -0.20f), new Vector2(-0.20f, -0.20f),
            new Vector2(0.30f, 0f), new Vector2(-0.30f, 0f), new Vector2(0f, 0.30f), new Vector2(0f, -0.30f),
        };

        foreach (var step in steps)
        {
            var candidate = (Vector2)basePos + step;
            lp.transform.position = candidate;
            Physics2D.SyncTransforms();

            overlapCount = lp.Collider.OverlapCollider(filter, results);
            if (overlapCount <= 0)
            {
                if (lp.MyPhysics?.body != null)
                {
                    lp.MyPhysics.body.position = candidate;
                }
                return;
            }
        }

        lp.transform.position = basePos;
        Physics2D.SyncTransforms();
    }

    public static void ReviveFromRewind(PlayerControl revived)
    {
        foreach (var drag in ModifierUtils.GetActiveModifiers<DragModifier>().ToList())
        {
            if (drag.BodyId == revived.PlayerId)
            {
                drag.Player.GetModifierComponent()?.RemoveModifier(drag);
                if (drag.Player.AmOwner && drag.Player.Data.Role is UndertakerRole)
                {
                    // TODO: Add Undertaker code here.
                }
            }
        }

        if (!revived.Data || !revived.Data.IsDead)
        {
            return;
        }

        GameHistory.ClearMurder(revived);

        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == revived.PlayerId);
        if (fakePlayer != null)
        {
            fakePlayer.Destroy();
            FakePlayer.FakePlayers.Remove(fakePlayer);
        }

        var body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == revived.PlayerId);
        var pos = revived.GetTruePosition();
        if (body != null)
        {
            pos = new Vector2(body.TruePosition.x, body.TruePosition.y + 0.3636f);
        }

        revived.Revive();

        revived.transform.position = pos;
        revived.NetTransform.SnapTo(pos);
        if (revived.AmOwner)
        {
            revived.NetTransform.RpcSnapTo(pos);
            var reviveFlashColor = new Color(0f, 0.5f, 0f, 1f);

            try
            {
                TouAudio.PlaySound(TouAudio.AltruistReviveSound);
                Coroutines.Start(MiscUtils.CoFlash(reviveFlashColor));
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.TimeLord.ToTextColor()}{TouLocale.GetParsed("TouRoleTimeLordRevivedNotif", "You were revived thanks to the Time Lord!")}</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.TimeLord.LoadAsset());
                notif.AdjustNotification();
            }
            catch
            {
               // ignored
            }
        }

        var roleWhenAlive = revived.GetRoleWhenAlive();
        revived.ChangeRole((ushort)roleWhenAlive.Role, false);

        if (!revived.AmOwner && !string.IsNullOrEmpty(revived.CurrentOutfit.PetId))
        {
            Coroutines.Start(CoRefreshPetState(revived));
        }

        if (body != null)
        {
            Object.Destroy(body.gameObject);
        }

        if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.PlayerId == revived.PlayerId)
        {
            ModCompatibility.ChangeFloor(revived.transform.position.y > -7);
        }
    }
}