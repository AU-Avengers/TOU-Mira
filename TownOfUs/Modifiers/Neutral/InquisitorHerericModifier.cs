using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class InquisitorHereticModifier : BaseModifier
{
    public override string ModifierName => "Inquisitor Heretic";
    public override bool HideOnUi => true;

    public RoleBehaviour TargetRole { get; set; }

    public override void OnActivate()
    {
        TargetRole = Player.Data.Role;
    }
}