namespace TownOfUs.Modifiers.Crewmate;

public sealed class DeputyRevealedModifier
    : BaseRevealModifier
{
    public override string ModifierName => TouLocale.Get("TouModifierDeputyRevealed");

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override bool RevealRole { get; set; } = true;
    public override bool Visible { get; set; } = true;

    public override void OnActivate()
    {
        base.OnActivate();
        SetNewInfo(true, roleTxt: TouLocale.Get("TouRoleDeputyRevealedText"));
    }
}