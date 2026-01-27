namespace TownOfUs.Modifiers.Crewmate;

public sealed class MarshalRevealModifier(RoleBehaviour role)
    : BaseRevealModifier
{
    public override string ModifierName => "Marshal Reveal";

    public ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.RemoveModifier;

    public RoleBehaviour? ShownRole { get; set; } = role;
    public bool RevealRole { get; set; } = true;
    public bool Visible { get; set; } = true;

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent?.RemoveModifier(this);
    }
}