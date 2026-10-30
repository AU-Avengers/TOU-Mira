using MiraAPI.Modifiers;
using System.Reflection;
using TownOfUs.Interfaces;
using TownOfUs.Modules.ControlSystem;

namespace TownOfUs.Utilities.ControlSystem;

public static class RemoteControlUtilities
{
    public static TControl? GetRemoteControl<TControl>(this PlayerControl player, Func<TControl, bool> predicate = null!)
        where TControl : class, IRemoteControl
    {
        if (player.Data?.Role is TControl role && (predicate == null || predicate(role)))
        {
            return role;
        }
        if (player.TryGetModifierOfType(out var mod, predicate))
        {
            return mod;
        }
        return null;
    }

    public static void EnsureControlRole(Type type)
    {
        if (!typeof(ITownOfUsControlRole).IsAssignableFrom(type) || !type.IsClass)
        {
            throw new ArgumentException("Type must be class and implement " + nameof(ITownOfUsControlRole), nameof(type));
        }
    }
}
