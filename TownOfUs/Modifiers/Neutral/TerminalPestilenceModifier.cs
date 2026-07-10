using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class TerminalPestilenceModifier(byte pestilenceId) : BaseModifier
{
    public override string ModifierName => "Terminal Pestilence";
    public override bool HideOnUi => true;

    public byte PestilenceId { get; } = pestilenceId;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
