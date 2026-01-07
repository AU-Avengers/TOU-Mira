using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SentryPortableCameraSecondaryButton : SentryPortableCameraButtonBase
{
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    protected override bool ShouldBeVisible(SentryRole role)
    {
        return AllCamerasPlaced();
    }
}

