namespace TownOfUs.Modifiers;

public sealed class FirstRoundIndicator: BaseRevealModifier
{
    public override string ModifierName => MiraLocaleManager.Get("TouFirstRoundDeathIndicator", "First Round Death Indicator");
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();
        SetNewInfo(false, $"\n<size=80%><color=yellow>{MiraLocaleManager.Get("FirstDeadText")}</color></size>");
    }
}