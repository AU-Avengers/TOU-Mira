using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Reflection;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.TimeLord;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;

namespace TownOfUs.Modules;

public static class TimeLordRewindSystem
{
    private enum SpecialAnim : byte
    {
        None = 0,
        Ladder = 1,
        Zipline = 2,
        Platform = 3,
        Vent = 4,
    }

    [Flags]
    private enum SnapshotState : byte
    {
        None = 0,
        InVent = 1 << 0,
        WalkingToVent = 1 << 1,
        InMovingPlat = 1 << 2,
        InvisibleAnim = 1 << 3,
        InMinigame = 1 << 4,
    }

    // Snapshot, CircularBuffer, TaskStepBuffer, and BodyPosBuffer moved to TimeLordSnapshotBuffer helper
    private static readonly TimeLord.CircularBuffer Buffer = new(1024);
    private static readonly TimeLord.TaskStepBuffer TaskBuffer = new(1024, 24);
    private static uint[] _trackedTaskIds = Array.Empty<uint>();
    private static int _trackedTaskCount;
    

    private static bool _colliderWasEnabled;
    private static bool _rewindDisabledLocalCollider;
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
    private static List<(TimeLordEvent Event, float UndoAt)>? _scheduledEventUndos;
    private static float _lastKillCooldownSampleTime;
    private static float _lastKillCooldownValue = -1f;
    private static float _lastKillButtonCooldownSampleTime;

    private readonly struct ButtonCooldownSample
    {
        public float Time { get; }
        public float Timer { get; }
        public bool EffectActive { get; }

        public ButtonCooldownSample(float time, float timer, bool effectActive)
        {
            Time = time;
            Timer = timer;
            EffectActive = effectActive;
        }
    }

    private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new();
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private sealed class ButtonCooldownSeries
    {
        public readonly List<ButtonCooldownSample> Samples = new(256);
        public int StartIndex;
    }

    private static readonly List<CustomActionButton> CachedKillLikeButtons = new(16);
    private static Type? _cachedKillLikeRoleType;
    private static float _lastKillLikeButtonsRefreshTime;

    private static readonly Dictionary<CustomActionButton, ButtonCooldownSeries> KillButtonCooldownHistory =
        new(ReferenceEqualityComparer<CustomActionButton>.Instance);

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

    private static readonly Dictionary<byte, TimeLord.BodyPosBuffer> HostBodyPosHistory = new();
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

    // CleanedBodies moved to TimeLordBodyManager helper

    private static bool _cachedHasTimeLord;
    private static float _nextHasTimeLordCheckTime;

    public static bool IsRewinding { get; private set; }
    public static byte SourceTimeLordId { get; private set; }
    public static float RewindEndTime { get; private set; }
    public static float RewindDuration { get; private set; }

    private static void LogBodyRestore(string msg)
    {
        var full = $"[TimeLordBodies] {msg}";
        try
        {
            TimeLordBodyManager.BodyLogger.LogError(full);
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

    private static void SeedCleanedBodiesFromHiddenBodies()
    {
        TimeLordBodyManager.SeedCleanedBodiesFromHiddenBodies();
    }


    public static void Reset()
    {
        IsRewinding = false;
        SourceTimeLordId = byte.MaxValue;
        RewindEndTime = 0f;
        RewindDuration = 0f;

        _colliderWasEnabled = false;
        _rewindDisabledLocalCollider = false;
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
        _lastKillCooldownSampleTime = 0f;
        _lastKillCooldownValue = -1f;
        _lastKillCooldownSampleTime = 0f;
        _lastKillCooldownValue = -1f;
        _lastKillButtonCooldownSampleTime = 0f;
        _cachedKillLikeRoleType = null;
        _lastKillLikeButtonsRefreshTime = 0f;
        CachedKillLikeButtons.Clear();
        KillButtonCooldownHistory.Clear();

        if (TutorialManager.InstanceExists)
        {
            TimeLordBodyManager.PruneCleanedBodies(maxAgeSeconds: 120f);
        }
        else
        {
            TimeLordBodyManager.Clear();
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
        
        // Only record snapshots when player is alive (not dead/ghost)
        if (lp.Data.IsDead)
        {
            return;
        }

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

        var flags = SnapshotState.None;
        var ventId = -1;
        if (lp.inVent)
        {
            flags |= SnapshotState.InVent;
        }
        if (lp.walkingToVent)
        {
            flags |= SnapshotState.WalkingToVent;
        }
        if ((flags & (SnapshotState.InVent | SnapshotState.WalkingToVent)) != 0)
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
            flags |= SnapshotState.InMovingPlat;
        }
        if (!lp.inVent && TimeLordAnimationUtilities.IsInInvisibleAnimation(lp))
        {
            flags |= SnapshotState.InvisibleAnim;
        }

        var inMinigame = Minigame.Instance != null || SpawnInMinigame.Instance != null;
        if (inMinigame)
        {
            flags |= SnapshotState.InMinigame;
        }

        Vector2 pos;
        if (lp.inMovingPlat || lp.walkingToVent)
        {
            pos = (Vector2)lp.transform.position;
        }
        else
        {
            pos = physics?.body != null ? physics.body.position : (Vector2)lp.transform.position;
        }

        if (inMinigame && _hasLastRecordedPos)
        {
            pos = _lastRecordedPos;
        }

        if (!IsValidSnapshotPos(lp, pos))
        {
            return;
        }

        Buffer.Add(new TimeLord.Snapshot(Time.time, pos, (TimeLord.SpecialAnim)anim, (TimeLord.SnapshotState)flags, ventId));
        _lastRecordedPos = pos;
        _hasLastRecordedPos = true;

        if (OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind)
        {
            RecordLocalTaskSteps(lp, samplesNeeded);
        }

        // Periodically sample kill cooldowns to capture natural decreases
        SampleKillCooldown(lp);

        // Periodically sample custom kill-button timers (Warlock/Glitch/etc).
        SampleKillButtonCooldowns(lp.Data.Role);
    }

    private static void RefreshKillLikeButtons(RoleBehaviour role, float now)
    {
        // Refresh at most once per second, or when role changes.
        if (_cachedKillLikeRoleType == role.GetType() && now - _lastKillLikeButtonsRefreshTime < 1.0f && CachedKillLikeButtons.Count > 0)
        {
            return;
        }

        _cachedKillLikeRoleType = role.GetType();
        _lastKillLikeButtonsRefreshTime = now;
        CachedKillLikeButtons.Clear();

        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button == null)
            {
                continue;
            }

            if (button is not IKillButton && button is not IDiseaseableButton)
            {
                continue;
            }

            if (!button.Enabled(role))
            {
                continue;
            }

            CachedKillLikeButtons.Add(button);
            // Ensure we have a series allocated for this button instance.
            if (!KillButtonCooldownHistory.ContainsKey(button))
            {
                KillButtonCooldownHistory[button] = new ButtonCooldownSeries();
            }
        }
    }

    private static void SampleKillButtonCooldowns(RoleBehaviour role)
    {
        if (IsRewinding)
        {
            return;
        }

        if (role == null)
        {
            return;
        }

        const float sampleInterval = 0.10f;
        var now = Time.time;
        if (now - _lastKillButtonCooldownSampleTime < sampleInterval)
        {
            return;
        }

        _lastKillButtonCooldownSampleTime = now;
        RefreshKillLikeButtons(role, now);

        var history = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 60f);
        const float retentionBuffer = 7.5f;
        var cutoff = now - (history + retentionBuffer);
        var maxSamples = (int)Math.Ceiling((history + retentionBuffer) / sampleInterval) + 4;

        for (var i = 0; i < CachedKillLikeButtons.Count; i++)
        {
            var button = CachedKillLikeButtons[i];
            if (button == null)
            {
                continue;
            }

            if (!KillButtonCooldownHistory.TryGetValue(button, out var series))
            {
                series = new ButtonCooldownSeries();
                KillButtonCooldownHistory[button] = series;
            }

            var samples = series.Samples;
            var start = series.StartIndex;

            while (start < samples.Count && samples[start].Time < cutoff)
            {
                start++;
            }
            series.StartIndex = start;

            if (samples.Count > start)
            {
                var last = samples[^1];
                if (Mathf.Abs(last.Timer - button.Timer) <= 0.01f && last.EffectActive == button.EffectActive)
                {
                    continue;
                }
            }

            samples.Add(new ButtonCooldownSample(now, button.Timer, button.EffectActive));

            var liveCount = samples.Count - series.StartIndex;
            if (liveCount > maxSamples)
            {
                series.StartIndex = samples.Count - maxSamples;
            }

            if (series.StartIndex > 256 && series.StartIndex > samples.Count / 2)
            {
                samples.RemoveRange(0, series.StartIndex);
                series.StartIndex = 0;
            }
        }
    }

    private static int FindLastSampleIndexAtOrBefore(List<ButtonCooldownSample> samples, int startIndex, float time)
    {
        var lo = startIndex;
        var hi = samples.Count - 1;
        var ans = -1;
        while (lo <= hi)
        {
            var mid = lo + ((hi - lo) / 2);
            if (samples[mid].Time <= time)
            {
                ans = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }
        return ans;
    }

    private static void RestoreKillButtonCooldownsForSnapshotTime(RoleBehaviour role, float snapshotTime)
    {
        if (role == null)
        {
            return;
        }

        RefreshKillLikeButtons(role, Time.time);

        for (var i = 0; i < CachedKillLikeButtons.Count; i++)
        {
            var button = CachedKillLikeButtons[i];
            if (button == null)
            {
                continue;
            }

            if (!KillButtonCooldownHistory.TryGetValue(button, out var series))
            {
                continue;
            }

            var samples = series.Samples;
            var start = series.StartIndex;
            if (samples.Count <= start)
            {
                continue;
            }

            var idx = FindLastSampleIndexAtOrBefore(samples, start, snapshotTime);
            if (idx < 0)
            {
                continue;
            }

            var sample = samples[idx];
            var restoredTimer = sample.Timer;
            
            // Prevent insta-killing after rewind
            var wasOnCooldown = false;
            if (restoredTimer <= 0f || restoredTimer < 0.1f)
            {
                for (var j = idx - 1; j >= start; j--)
                {
                    if (samples[j].Timer > 0.1f)
                    {
                        wasOnCooldown = true;
                        break;
                    }
                }
                
                if (wasOnCooldown)
                {
                    restoredTimer = button.Cooldown;
                }
            }
            
            if (Mathf.Abs(button.Timer - restoredTimer) > 0.01f || button.EffectActive != sample.EffectActive)
            {
                button.Timer = restoredTimer;
                button.EffectActive = sample.EffectActive;
            }
        }
    }

    private static void SampleKillCooldown(PlayerControl player)
    {
        // Don't sample during rewind
        if (IsRewinding)
        {
            return;
        }

        if (player == null || !player.Data.Role.CanUseKillButton)
        {
            return;
        }

        const float sampleInterval = 0.5f; // Sample every 0.5 seconds
        var now = Time.time;
        
        if (now - _lastKillCooldownSampleTime < sampleInterval)
        {
            return;
        }

        var currentCooldown = player.killTimer;
        
        // Only record if the cooldown has changed significantly (more than 0.1 seconds)
        if (_lastKillCooldownValue >= 0f && Mathf.Abs(currentCooldown - _lastKillCooldownValue) > 0.1f)
        {
            TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordKillCooldown(
                player, _lastKillCooldownValue, currentCooldown);
        }

        _lastKillCooldownValue = currentCooldown;
        _lastKillCooldownSampleTime = now;
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
        _rewindDisabledLocalCollider = false;

        TimeLordBodyManager.ResetRestoredThisRewind();

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
                    // ignored
                }
                PlayerControl.LocalPlayer.onLadder = false;
            }

            if (PlayerControl.LocalPlayer.Collider != null)
            {
                _colliderWasEnabled = PlayerControl.LocalPlayer.Collider.enabled;
                PlayerControl.LocalPlayer.Collider.enabled = false;
                _rewindDisabledLocalCollider = true;
            }
        }

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.TimeLord, duration, 0.25f));

        ConfigureLocalBodyRestores(duration, history);
        
        // Schedule event-based undos
        var eventQueue = TownOfUs.Events.Crewmate.TimeLordEventHandlers.GetEventQueue();
        var historySeconds = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 120f);
        _scheduledEventUndos = eventQueue.GetUndoSchedule(Time.time, duration, historySeconds);

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
                buf = new TimeLord.BodyPosBuffer(cap);
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
                $"ConfigureLocalBodyRestores: skipped (option={OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind}, cleanedBodies={TimeLordBodyManager.GetCleanedBodyCount()})");
            return;
        }

        if (!TimeLordBodyManager.HasCleanedBodies())
        {
            SeedCleanedBodiesFromHiddenBodies();
        }

        if (!TimeLordBodyManager.HasCleanedBodies())
        {
            _localBodyRestores = null;
            LogBodyRestore("ConfigureLocalBodyRestores: no cleaned bodies to schedule after seeding attempt");
            return;
        }

        var nowTime = Time.time;
        var cutoffTime = nowTime - historySeconds;

        var schedule = TimeLordBodyManager.GetCleanedBodiesForScheduling(cutoffTime)
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
            $"ConfigureLocalBodyRestores: scheduled={schedule.Count} (cleanedBodiesTotal={TimeLordBodyManager.GetCleanedBodyCount()}, cutoffTime={cutoffTime:0.000}, nowTime={nowTime:0.000})");
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

    private static void ApplyVentSnapshotState(PlayerControl lp, TimeLord.Snapshot snap)
    {
        var wantInVent = (snap.Flags & (TimeLord.SnapshotState)SnapshotState.InVent) != 0;

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
        lp.walkingToVent = (snap.Flags & (TimeLord.SnapshotState)SnapshotState.WalkingToVent) != 0;
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

        var unsafeNow = lp.walkingToVent || lp.inMovingPlat || (!lp.inVent && TimeLordAnimationUtilities.IsInInvisibleAnimation(lp));
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

            if (_hasFinalSnapPos && IsValidSnapshotPos(lp, _finalSnapPos))
            {
                lp.transform.position = _finalSnapPos;
                if (physics.body != null)
                {
                    physics.body.position = _finalSnapPos;
                }
                lp.NetTransform?.SnapTo(_finalSnapPos);
            }
            else if (_hasFinalSnapPos)
            {
                // Invalid position, clear it
                _hasFinalSnapPos = false;
                _finalSnapPos = default;
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
                        // Check if victim exists, is connected, and is still dead before reviving
                        // Double-check the victim is still dead right before reviving (race condition protection)
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
                    TimeLordBodyManager.RestoreCleanedBody(entry.BodyId);
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

        // Process event-based undo events
        if (_scheduledEventUndos != null && _scheduledEventUndos.Count > 0)
        {
            var elapsed = Time.time - _rewindStartTime;
            for (var i = _scheduledEventUndos.Count - 1; i >= 0; i--)
            {
                var (evt, undoAt) = _scheduledEventUndos[i];
                if (elapsed + 0.0001f >= undoAt)
                {
                    // Fire the undo event through MiraEventManager
                    var undoEvent = CreateUndoEvent(evt);
                    MiraAPI.Events.MiraEventManager.InvokeEvent(undoEvent);
                    _scheduledEventUndos.RemoveAt(i);
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

        if (Buffer.TryPeekLast(out var peek) && (peek.Flags & (TimeLord.SnapshotState)SnapshotState.InMinigame) != 0)
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

        TimeLord.Snapshot snap = default;
        TimeLord.TaskStepSnapshot taskSnap = default;
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
            lp.onLadder = snap.Anim == (TimeLord.SpecialAnim)SpecialAnim.Ladder;
        }

        ApplyVentSnapshotState(lp, snap);

        if (_lastRewindAnim == SpecialAnim.Ladder && snap.Anim != (TimeLord.SpecialAnim)SpecialAnim.Ladder)
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

        _lastRewindAnim = (SpecialAnim)snap.Anim;

        if (lp.inVent && Vent.currentVent != null)
        {
            var vpos = (Vector2)Vent.currentVent.transform.position;
            _finalSnapPos = vpos;
            _hasFinalSnapPos = true;
            _finalSnapFlags = (SnapshotState)snap.Flags;
            _finalSnapVentId = snap.VentId;

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
        _finalSnapFlags = (SnapshotState)snap.Flags;
        _finalSnapVentId = snap.VentId;

        if (OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind && _trackedTaskCount > 0 && taskSnap.Steps != null)
        {
            ApplyLocalTaskSteps(PlayerControl.LocalPlayer, taskSnap);
        }

        var isLadder = snap.Anim == (TimeLord.SpecialAnim)SpecialAnim.Ladder;
        if (isLadder)
        {
            var helperAnim = snap.Anim;
            TimeLordAnimationUtilities.ApplySpecialAnimation(physics, helperAnim, delta);
        }
        TimeLordParasiteMovementUtilities.ApplyRewindMovement(physics, snap.Pos, curPos, isLadder);

        // Restore kill cooldown based on snapshot time
        RestoreKillCooldownForSnapshot(lp, snap.Time);

        // Restore custom kill-button timers (Warlock/Glitch/etc) based on snapshot time
        RestoreKillButtonCooldownsForSnapshotTime(lp.Data.Role, snap.Time);

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
        _scheduledEventUndos = null;
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

        var endedInVent = ((TimeLord.SnapshotState)_finalSnapFlags & (TimeLord.SnapshotState)SnapshotState.InVent) != 0;
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

        // Only apply final snap position if we're still in a valid game state and the position is valid
        if (_hasFinalSnapPos && !endedInVent && 
            AmongUsClient.Instance != null && 
            (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started || 
             AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) &&
            IsValidSnapshotPos(lp, _finalSnapPos))
        {
            lp.transform.position = _finalSnapPos;
            if (lp.MyPhysics?.body != null)
            {
                lp.MyPhysics.body.position = _finalSnapPos;
            }
            lp.NetTransform?.SnapTo(_finalSnapPos);
        }
        else if (_hasFinalSnapPos)
        {
            // Invalid position or game state, clear it
            _hasFinalSnapPos = false;
            _finalSnapPos = default;
        }

        if (lp.Collider != null)
        {
            // Only restore collider state if rewind actually disabled it.
            // If the local player was a ghost when rewind started and got revived mid-rewind,
            // forcing collider.enabled from a stale cached value can leave them in a "ghost collision" state.
            if (_rewindDisabledLocalCollider)
            {
                lp.Collider.enabled = endedInVent || _colliderWasEnabled;
            }
            else if (lp.Data != null && !lp.Data.IsDead)
            {
                lp.Collider.enabled = true;
            }

            // Ensure living players are not left as triggers (ghost-like collision)
            if (lp.Data != null && !lp.Data.IsDead)
            {
                lp.Collider.isTrigger = false;
            }

            _colliderWasEnabled = false;
            _rewindDisabledLocalCollider = false;

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

        if (!TimeLordBodyManager.HasCleanedBodies())
        {
            return;
        }

        foreach (var rec in TimeLordBodyManager.GetCleanedBodiesRestoredThisRewind().ToList())
        {
            if (rec == null ||
                !rec.RestoredThisRewind ||
                rec.Source != TimeLordBodyManager.CleanedBodySource.Rotting ||
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
        _finalSnapFlags = (SnapshotState)TimeLord.SnapshotState.None;
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
            if (_rewindDisabledLocalCollider)
            {
                lp.Collider.enabled = _colliderWasEnabled;
            }
            else if (lp.Data != null && !lp.Data.IsDead)
            {
                lp.Collider.enabled = true;
            }

            if (lp.Data != null && !lp.Data.IsDead)
            {
                lp.Collider.isTrigger = false;
            }
            _colliderWasEnabled = false;
            _rewindDisabledLocalCollider = false;
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

    private static void ApplyLocalTaskSteps(PlayerControl lp, TimeLord.TaskStepSnapshot snap)
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
            snitch.RecalculateTaskStage(silent: IsRewinding);
        }
    }

    // Animation methods moved to TimeLordAnimationUtilities helper

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

    private static SnapshotState _finalSnapFlags;
    private static SpecialAnim _lastRewindAnim = SpecialAnim.None;
    private static int _finalSnapVentId = -1;

    private static void RestoreKillCooldownForSnapshot(PlayerControl player, float snapshotTime)
    {
        if (player == null || !player.AmOwner || !player.Data.Role.CanUseKillButton)
        {
            return;
        }

        if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f)
        {
            return;
        }

        var eventQueue = TownOfUs.Events.Crewmate.TimeLordEventHandlers.GetEventQueue();
        var historySeconds = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 120f);
        var startTime = snapshotTime - historySeconds;
        var endTime = snapshotTime;
        
        var killCooldownEvents = eventQueue.GetEvents<TownOfUs.Events.TouEvents.TimeLordKillCooldownEvent>(startTime, endTime);
        
        TownOfUs.Events.TouEvents.TimeLordKillCooldownEvent? mostRecentEvent = null;
        foreach (var kcEvent in killCooldownEvents)
        {
            if (kcEvent.Player == player && kcEvent.Time <= snapshotTime && 
                (mostRecentEvent == null || kcEvent.Time > mostRecentEvent.Time))
            {
                mostRecentEvent = kcEvent;
            }
        }
        
        if (mostRecentEvent != null)
        {
            var timeSinceEvent = snapshotTime - mostRecentEvent.Time;
            var expectedCooldown = Mathf.Max(0f, mostRecentEvent.CooldownAfter - timeSinceEvent);
            
            var maxKillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            
            // Prevent insta-killing after rewind
            if ((expectedCooldown <= 0f || expectedCooldown < 0.1f) && 
                mostRecentEvent.CooldownAfter > 0.1f)
            {
                expectedCooldown = maxKillCooldown;
            }
            
            var maxvalue = expectedCooldown > maxKillCooldown
                ? expectedCooldown + 1f
                : maxKillCooldown;
            
            player.killTimer = Mathf.Clamp(expectedCooldown, 0, maxvalue);
            if (HudManager.Instance != null && HudManager.Instance.KillButton != null)
            {
                HudManager.Instance.KillButton.SetCoolDown(player.killTimer, maxvalue);
            }
        }
    }

    private static TimeLordUndoEvent CreateUndoEvent(TimeLordEvent evt)
    {
        return evt switch
        {
            TimeLordVentEnterEvent e => new TimeLordVentEnterUndoEvent(e),
            TimeLordVentExitEvent e => new TimeLordVentExitUndoEvent(e),
            TimeLordTaskCompleteEvent e => new TimeLordTaskCompleteUndoEvent(e),
            TimeLordBodyCleanedEvent e => new TimeLordBodyCleanedUndoEvent(e),
            TimeLordKillEvent e => new TimeLordKillUndoEvent(e),
            TimeLordChefCookEvent e => new TimeLordChefCookUndoEvent(e),
            TimeLordChefServeEvent e => new TimeLordChefServeUndoEvent(e),
            TimeLordKillCooldownEvent e => new TimeLordKillCooldownUndoEvent(e),
            _ => throw new ArgumentException($"Unknown event type: {evt.GetType()}")
        };
    }

    private static void AdvanceFinalSnapToSafeIfNeeded(PlayerControl lp)
    {
        var unsafeNow = lp.walkingToVent || lp.inMovingPlat || (!lp.inVent && TimeLordAnimationUtilities.IsInInvisibleAnimation(lp));
        var unsafeLanding = (((TimeLord.SnapshotState)_finalSnapFlags & ((TimeLord.SnapshotState)SnapshotState.WalkingToVent | (TimeLord.SnapshotState)SnapshotState.InMovingPlat | (TimeLord.SnapshotState)SnapshotState.InvisibleAnim)) != 0);
        if (!unsafeNow && !unsafeLanding)
        {
            return;
        }

        while (Buffer.TryPopLast(out var snap))
        {
            var snapUnsafe = (snap.Flags & ((TimeLord.SnapshotState)SnapshotState.WalkingToVent | (TimeLord.SnapshotState)SnapshotState.InMovingPlat | (TimeLord.SnapshotState)SnapshotState.InvisibleAnim)) != 0;
            if (snapUnsafe)
            {
                continue;
            }

            _finalSnapPos = snap.Pos;
            _finalSnapFlags = (SnapshotState)snap.Flags;
            _finalSnapVentId = snap.VentId;
            _hasFinalSnapPos = true;
            break;
        }
    }

    private static void PopOutOfVentIfNeeded(PlayerControl lp)
    {
        if (!lp)
        {
            return;
        }

        if (((TimeLord.SnapshotState)_finalSnapFlags & (TimeLord.SnapshotState)SnapshotState.InVent) != 0)
        {
            return;
        }

        var shouldPop = lp.inVent || lp.walkingToVent || (((TimeLord.SnapshotState)_finalSnapFlags & (TimeLord.SnapshotState)SnapshotState.WalkingToVent) != 0);
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
                snitch.RecalculateTaskStage(silent: IsRewinding);
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
                    CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrag();
                }
            }
        }

        if (!revived.Data || !revived.Data.IsDead)
        {
            return;
        }

        var roleWhenAlive = revived.GetRoleWhenAlive();

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

        // Set position before Revive() so the PlayerReviveEvent handler can sync physics correctly
        revived.transform.position = pos;
        revived.Revive();

        // Swap off any ghost-role state ASAP; ghost-role patches can disable colliders during ResetMoveState.
        if (roleWhenAlive != null)
        {
            revived.ChangeRole((ushort)roleWhenAlive.Role, false);
        }

        // Force collision/physics back into a living state (Time Lord revive can happen mid-rewind,
        // where collider state caching/restoration differs from normal revives).
        if (revived.Collider != null)
        {
            revived.Collider.enabled = true;
            revived.Collider.isTrigger = false;
        }
        if (revived.MyPhysics?.body != null)
        {
            revived.MyPhysics.body.position = pos;
        }
        Physics2D.SyncTransforms();

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

        // If roleWhenAlive was null above, preserve existing behavior (no role change).
        // Re-assert collision state after snaps, to protect against other patches toggling collider.
        if (revived.Collider != null && revived.Data != null && !revived.Data.IsDead)
        {
            revived.Collider.enabled = true;
            revived.Collider.isTrigger = false;
        }

        if (revived.AmOwner && PlayerControl.LocalPlayer != null && revived.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            TryUnstuckLocalPlayer(revived);
        }

        if (!revived.AmOwner && !string.IsNullOrEmpty(revived.CurrentOutfit.PetId))
        {
            Coroutines.Start(CoRefreshPetState(revived));
        }

        if (body != null)
        {
            Object.Destroy(body.gameObject);
        }

        if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == revived.PlayerId)
        {
            ModCompatibility.ChangeFloor(revived.transform.position.y > -7);
        }
    }
}