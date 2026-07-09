using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Neutral;

public sealed class PestilenceStackModifier(byte pestilenceId) : BaseModifier
{
    public override string ModifierName => "Pestilence Stack";
    public override bool HideOnUi => true;

    public byte PestilenceId { get; } = pestilenceId;

    public override void OnActivate()
    {
        base.OnActivate();

        if (PlayerControl.LocalPlayer.PlayerId == PestilenceId)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Pestilence));
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
