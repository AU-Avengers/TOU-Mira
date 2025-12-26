using MiraAPI.Hud;

namespace TownOfUs.Modules;

/// <summary>
/// Controls whether the TownOfUs freeplay debug buttons (bottom-left) are shown.
/// Freeplay is identified by Tutorial mode (see <c>TutorialManager.InstanceExists</c>).
/// </summary>
public static class FreeplayButtonsVisibility
{
    public static bool Hidden { get; private set; }

    public static void Toggle()
    {
        Hidden = !Hidden;
        Apply();
    }

    public static void Apply()
    {
        if (!TutorialManager.InstanceExists || PlayerControl.LocalPlayer?.Data?.Role == null)
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
                button.SetActive(!Hidden, role);
            }
            catch
            {
                // ignored
            }
        }
    }
}