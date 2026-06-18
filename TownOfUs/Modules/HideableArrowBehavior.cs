using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Modules;

public class HideableArrowBehavior : ArrowBehaviour
{
    public PlayerControl? Player { get; set; }

    public override void UpdatePosition()
    {
        var hide = SwoopModifier.CanBeTracked == SwoopTracking.Never &&
                   Player != null && Player.HasModifier<SwoopModifier>();

        SetImageEnabled(!hide);
        if (!hide)
        {
            base.UpdatePosition();
        }
    }
}
