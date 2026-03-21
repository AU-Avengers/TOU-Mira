namespace TownOfUs.Utilities;

/// <summary>
/// Tracks all FakeChat messages received during a meeting.
/// Cleared when voting completes. Used by the /info command.
/// </summary>
public static class FakeChatHistory
{
    private static readonly List<(string Title, string Message)> _entries = new();

    /// <summary>
    /// Set to true while /info is replaying entries so the patch doesn't
    /// re-record the replayed messages back into history.
    /// </summary>
    public static bool IsReplaying { get; set; }

    /// <summary>Whether any role info messages have been recorded this meeting.</summary>
    public static bool HasInfo => _entries.Count > 0;

    /// <summary>Records a (title, message) pair.</summary>
    public static void Record(string title, string message)
    {
        _entries.Add((title, message));
    }

    /// <summary>Returns all recorded entries (read-only).</summary>
    public static IReadOnlyList<(string Title, string Message)> GetEntries() => _entries.AsReadOnly();

    /// <summary>Clears all entries.</summary>
    public static void Clear()
    {
        _entries.Clear();
    }
}