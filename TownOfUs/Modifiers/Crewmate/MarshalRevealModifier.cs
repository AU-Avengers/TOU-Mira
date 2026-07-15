namespace TownOfUs.Modifiers.Crewmate;

public sealed class MarshalRevealModifier(RoleBehaviour role)
    : BaseRevealModifier
{
    public override string ModifierName => "Marshal Reveal";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.RemoveModifier;

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;
    public override bool Visible { get; set; } = true;

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent?.RemoveModifier(this);
    }
}