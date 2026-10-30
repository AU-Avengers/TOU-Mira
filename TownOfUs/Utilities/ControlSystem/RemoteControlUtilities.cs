using MiraAPI.Modifiers;
using TownOfUs.Interfaces;

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
}
