using Rewired;

namespace TownOfUs;

public static class KeybindHelpers
{
    public static string PrettyKeyName(KeyboardKeyCode key)
    {
        var name = key.ToString();

        if (name.StartsWith("Alpha", StringComparison.InvariantCulture) && name.Length == 6 && char.IsDigit(name[5]))
        {
            return name.Substring(5);
        }

        if (name.StartsWith("Keypad", StringComparison.InvariantCulture) && name.Length == 7 && char.IsDigit(name[6]))
        {
            return name.Substring(6);
        }

        return name;
    }
}
