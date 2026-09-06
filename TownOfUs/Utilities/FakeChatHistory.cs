namespace TownOfUs.Utilities;
public static class FakeChatHistory
{
    private static readonly Dictionary<int, List<(NetworkedPlayerInfo Player, string Title, string Message)>> _rounds = [];
    private static int _currentRound = 1;
    public static bool IsReplaying { get; set; }
    public static int CurrentRound => _currentRound;
    public static bool HasInfo(int? round = null) =>
        _rounds.TryGetValue(round ?? _currentRound, out var entries) && entries.Count > 0;
    public static void Record(NetworkedPlayerInfo player, string title, string message)
    {
        if (!_rounds.TryGetValue(_currentRound, out var entries))
        {
            entries = [];
            _rounds[_currentRound] = entries;
        }

        entries.Add((player, title, message));
    }
    public static IReadOnlyList<(NetworkedPlayerInfo Player, string Title, string Message)> GetEntries(int? round = null) =>
        _rounds.TryGetValue(round ?? _currentRound, out var entries) ? entries.AsReadOnly() : [];
    public static void Clear()
    {
        _currentRound++;
    }
    public static void ClearAll()
    {
        _rounds.Clear();
        _currentRound = 1;
    }
}