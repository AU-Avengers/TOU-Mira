namespace TownOfUs.Modules.Cosmetics;

public class Group(Dictionary<string, string> groupMap)
{
    private readonly List<string> _groupIds = [];

    public int Count => _groupIds.Count;

    public void AddGroup(string id)
    {
        groupMap.TryAdd(id, id);
        if (!_groupIds.Contains(id))
        {
            _groupIds.Add(id);
        }
    }

    public string GetGroupIdByIndex(int index)
    {
        return _groupIds[index];
    }

    public string GetGroupNameByIndex(int index)
    {
        return groupMap[_groupIds[index]];
    }
}