namespace TownOfUs.Modules.Cosmetics;

public static class Names
{
    public static string Normalize(
        string name,
        string type,
        string group = "default"
    )
    {
        return $"toum.{group}.{type}.{name.ToLower().Replace(" ", "_")}";
    }

    public static string GetGroup(string id)
    {
        return id.Split('.')[1];
    }
}