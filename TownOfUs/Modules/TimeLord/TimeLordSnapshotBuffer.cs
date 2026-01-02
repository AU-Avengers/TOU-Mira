using UnityEngine;

namespace TownOfUs.Modules.TimeLord;

/// <summary>
/// Snapshot data structures and circular buffer for Time Lord position tracking.
/// </summary>
internal enum SpecialAnim : byte
{
    None = 0,
    Ladder = 1,
    Zipline = 2,
    Platform = 3,
    Vent = 4,
}

[Flags]
internal enum SnapshotState : byte
{
    None = 0,
    InVent = 1 << 0,
    WalkingToVent = 1 << 1,
    InMovingPlat = 1 << 2,
    InvisibleAnim = 1 << 3,
    InMinigame = 1 << 4,
}

internal readonly struct Snapshot
{
    public readonly float Time;
    public readonly Vector2 Pos;
    public readonly SpecialAnim Anim;
    public readonly SnapshotState Flags;
    public readonly int VentId;

    public Snapshot(float time, Vector2 pos, SpecialAnim anim, SnapshotState flags, int ventId)
    {
        Time = time;
        Pos = pos;
        Anim = anim;
        Flags = flags;
        VentId = ventId;
    }
}

internal readonly struct TaskStepSnapshot
{
    public readonly byte[] Steps;

    public TaskStepSnapshot(byte[] steps)
    {
        Steps = steps;
    }
}

internal sealed class CircularBuffer
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

        while (_count > 0)
        {
            var idx = _start % _items.Length;
            if (_items[idx].Time >= cutoffTime)
            {
                break;
            }

            _start = (_start + 1) % _items.Length;
            _count--;
        }
    }
}

internal sealed class TaskStepBuffer
{
    private byte[][] _steps;
    private int _start;
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

internal sealed class BodyPosBuffer
{
    private Vector2[] _items;
    private int _start;
    private int _count;

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