using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class HaunterArrowModifier(PlayerControl owner, Color color) : ArrowTargetModifier(owner, color, 0)
{
    public override string ModifierName => "Haunter Arrow";

    public override void OnActivate()
    {
        if (ShouldShowArrow())
        {
            base.OnActivate();
        }
    }

    public override void FixedUpdate()
    {
        if (!ShouldShowArrow())
        {
            return;
        }

        if (Arrow == null)
        {
            OnActivate();
        }

        if (Arrow != null)
        {
            base.FixedUpdate();
        }
    }

    private bool ShouldShowArrow()
    {
        if (Owner == null || Owner.Data == null || PlayerControl.LocalPlayer == null)
        {
            return false;
        }

        return PlayerControl.LocalPlayer.PlayerId == Owner.PlayerId;
    }
}