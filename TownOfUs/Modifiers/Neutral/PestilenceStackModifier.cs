using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

// Applied to anyone who interacts with the Pestilence while Legacy Mode is off.
// Only visible to the Pestilence (see PlayerRoleTextExtensions.UpdateStatusSymbols).
// Killing a marked player resets the Pestilence's kill cooldown to 0 (chain kill).
public sealed class PestilenceStackModifier(byte pestilenceId) : BaseModifier
{
    public override string ModifierName => "Pestilence Stack";
    public override bool HideOnUi => true;

    public byte PestilenceId { get; } = pestilenceId;
}
