using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SentryPortableCameraSecondaryButton : SentryPortableCameraButtonBase, ILegacyCapable
{
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    protected override bool ShouldBeVisible(SentryRole role)
    {
        return AllCamerasPlaced();
    }
}

