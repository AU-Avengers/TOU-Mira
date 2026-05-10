using System.Globalization;

namespace TownOfUs.Modules.Cosmetics;

public static class Names
{
    public static string Normalize(
        string name,
        string type,
        string group = "default"
    )
    {
        return $"toum.{group}.{type}.{name.ToLower(CultureInfo.InvariantCulture).Replace(" ", "_")}";
    }

    public static string GetGroup(string id)
    {
        return id.Split('.')[1];
    }
}