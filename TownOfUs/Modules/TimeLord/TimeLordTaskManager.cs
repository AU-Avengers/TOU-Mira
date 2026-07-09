namespace TownOfUs.Modules.TimeLord;

/// <summary>
/// Manages task completion tracking and undo for Time Lord rewind system.
/// </summary>
internal static class TimeLordTaskManager
{
    private readonly record struct HostTaskCompletion(byte PlayerId, uint TaskId, DateTime TimeUtc);

    private sealed record ScheduledTaskUndo(byte PlayerId, uint TaskId, float TriggerAtSeconds)
    {
        public bool Done { get; set; }
    }

    private static readonly List<HostTaskCompletion> HostTaskCompletions = [with(64)];
    private static List<ScheduledTaskUndo>? _hostTaskUndos;

    public static void ClearHostTaskHistory()
    {
        HostTaskCompletions.Clear();
    }

    public static void RecordHostTaskCompletion(PlayerControl player, PlayerTask task)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!TimeLordRewindSystem.MatchHasTimeLord())
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

        _hostTaskUndos = [with(schedule.Count)];
        foreach (var (playerId, taskId, triggerAt) in schedule)
        {
            _hostTaskUndos.Add(new ScheduledTaskUndo(playerId, taskId, triggerAt));
        }
    }

    public static void ConfigureHostTaskUndosFromHistory(float durationSeconds, float historySeconds)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
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

    public static void ProcessScheduledTaskUndos(float elapsed)
    {
        if (_hostTaskUndos == null || _hostTaskUndos.Count == 0)
        {
            return;
        }

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
                TimeLordRewindSystem.UndoTask(entry.PlayerId, entry.TaskId);
            }
        }
    }

    public static void Clear()
    {
        _hostTaskUndos = null;
    }
}