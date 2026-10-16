using System;
using System.Linq;
using BepInEx.Unity.IL2CPP;

namespace TownOfUs.Modules;

/// <summary>
/// Provides a safe runtime check for whether the AchievementsAPI plugin is loaded,
/// without requiring a hard BepInEx dependency on it.
/// </summary>
public static class AchievementsAPIHelper
{
    /// <summary>
    /// Returns true when the AchievementsAPI assembly is present and its plugin has been loaded.
    /// </summary>
    public static bool IsLoaded
    {
        get
        {
            try
            {
                return IL2CPPChainloader.Instance.Plugins.Values.Any(static p =>
                    string.Equals(p.Instance?.GetType().Assembly.GetName().Name, "AchievementsAPI", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }
    }
}
