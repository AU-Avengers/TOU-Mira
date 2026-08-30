namespace TownOfUs.Modifiers.Crewmate;

public sealed class DeputyRevealedModifier(RoleBehaviour role)
    : BaseRevealModifier
{
    public override string ModifierName => MiraLocaleManager.Get("TouModifierDeputyRevealed");


    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override RoleBehaviour? ShownRole { get; set; } = role;

    public override bool RevealRole { get; set; } = true;
    public override bool Visible { get; set; } = true;

    public override void OnActivate()
    {
        base.OnActivate();
        SetNewInfo(true, roleTxt: MiraLocaleManager.Get("TownOfUsMira.Role.DeputyRevealedText"));
    }
}