using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Neutral;

// Applied to anyone who interacts with the Pestilence while Legacy Mode is off.
// Only visible to the Pestilence (see PlayerRoleTextExtensions.UpdateStatusSymbols).
// Killing a marked player resets the Pestilence's kill cooldown to 0 (chain kill).
public sealed class PestilenceStackModifier(byte pestilenceId) : BaseModifier
{
    public override string ModifierName => "Pestilence Stack";
    public override bool HideOnUi => true;

    public byte PestilenceId { get; } = pestilenceId;

    public override void OnActivate()
    {
        base.OnActivate();

        // Flash the Pestilence's own screen so they know someone just interacted with them.
        if (PlayerControl.LocalPlayer.PlayerId == PestilenceId)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Pestilence));
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        // The stack is consumed when the marked player dies (also clears the name symbol).
        ModifierComponent!.RemoveModifier(this);
    }
}
