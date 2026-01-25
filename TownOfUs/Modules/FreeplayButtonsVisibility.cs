using BepInEx.Configuration;
using MiraAPI.Hud;

namespace TownOfUs.Modules;

/// <summary>
/// Controls whether the TownOfUs freeplay debug buttons (bottom-left) are shown.
/// Freeplay is identified by Tutorial mode (see <c>TutorialManager.InstanceExists</c>).
/// </summary>
public static class FreeplayButtonsVisibility
{
    public static ConfigEntry<bool> PracticeModeToggle =>
        LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowPracticeButtons;
    public static bool Hidden => !PracticeModeToggle.Value;

    public static void Toggle()
    {
        PracticeModeToggle.Value = !PracticeModeToggle.Value;
        Apply();
    }

    public static void Apply()
    {
        if ((!TutorialManager.InstanceExists && !MultiplayerFreeplayMode.Enabled) ||
            PlayerControl.LocalPlayer?.Data?.Role == null)
        {
            return;
        }

        var role = PlayerControl.LocalPlayer.Data.Role;
        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button == null)
            {
                continue;
            }

            if (button.GetType().Namespace != "TownOfUs.Buttons.BaseFreeplay")
            {
                continue;
            }

            try
            {
                button.SetActive(PracticeModeToggle.Value, role);
            }
            catch
            {
                // ignored
            }
        }
    }
}