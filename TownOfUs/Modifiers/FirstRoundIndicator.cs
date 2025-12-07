namespace TownOfUs.Modifiers;

public sealed class FirstRoundIndicator(): RevealModifier((int)ChangeRoleResult.Nothing, true, null)
{
    public override string ModifierName => "...";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();
        SetNewInfo(false, $"\n<size=80%><color=yellow>{TouLocale.GetParsed("FirstDeadText")}</color></size>");
    }
}