using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class PestilenceStackModifier(byte pestilenceId) : BaseModifier
{
    public override string ModifierName => "Pestilence Stack";
    public override bool HideOnUi => true;

    public byte PestilenceId { get; } = pestilenceId;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
