using TownOfUs.Events.TouEvents;
using UnityEngine;

namespace TownOfUs.Modules.TimeLord;

/// <summary>
/// Manages the registry of Time Lord events for rewind processing.
/// Events are stored in time order and can be undone during rewind.
/// </summary>
public sealed class TimeLordEventRegistry
{
    private readonly List<QueuedEvent> _events = new();
    private readonly Dictionary<Type, Action<TimeLordEvent>> _undoHandlers = new();

    private sealed class QueuedEvent
    {
        public TimeLordEvent Event { get; }
        public float Time { get; }
        public bool Undone { get; set; }

        public QueuedEvent(TimeLordEvent evt, float time)
        {
            Event = evt;
            Time = time;
            Undone = false;
        }
    }

    public void RegisterUndoHandler<T>(Action<T> handler) where T : TimeLordEvent
    {
        _undoHandlers[typeof(T)] = evt => handler((T)evt);
    }

    public void RecordEvent(TimeLordEvent evt)
    {
        if (evt == null)
        {
            return;
        }

        _events.Add(new QueuedEvent(evt, evt.Time));
        
        // Keep only events within the rewind history window
        var cutoff = Time.time - 120f; // Max 120 seconds history
        _events.RemoveAll(e => e.Time < cutoff);
    }

    public void Clear()
    {
        _events.Clear();
    }

    /// <summary>
    /// Gets events that should be undone during rewind, ordered by when they should be undone.
    /// Events are undone in reverse chronological order (newest first).
    /// </summary>
    public List<(TimeLordEvent Event, float UndoAt)> GetUndoSchedule(float rewindStartTime, float rewindDuration, float historySeconds)
    {
        var cutoff = rewindStartTime - historySeconds;
        var schedule = new List<(TimeLordEvent, float)>();

        foreach (var queued in _events)
        {
            if (queued.Undone || queued.Time < cutoff)
            {
                continue;
            }

            // Calculate when to undo this event during rewind
            // Older events are undone later in the rewind
            var age = rewindStartTime - queued.Time;
            var undoAt = rewindDuration * (age / historySeconds);
            
            schedule.Add((queued.Event, undoAt));
        }

        // Sort by undo time (earliest first)
        schedule.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        return schedule;
    }

    public void MarkUndone(TimeLordEvent evt)
    {
        var queued = _events.FirstOrDefault(e => e.Event == evt);
        if (queued != null)
        {
            queued.Undone = true;
        }
    }

    public void ProcessUndoEvent(TimeLordUndoEvent undoEvent)
    {
        var originalType = undoEvent.OriginalEvent.GetType();
        
        if (_undoHandlers.TryGetValue(originalType, out var handler))
        {
            handler(undoEvent.OriginalEvent);
        }
    }
}