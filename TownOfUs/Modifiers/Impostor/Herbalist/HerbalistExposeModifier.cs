using MiraAPI.Modifiers;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Impostor.Herbalist;

public sealed class HerbalistExposedModifier(PlayerControl herbalist, bool isFfa)
    : BaseRevealModifier
{
    public override string ModifierName => "Exposed";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override bool RevealRole { get; set; } = true;
    public override bool Visible { get; set; } = true;
    public bool FreeForAllActive => isFfa;
    public PlayerControl Herbalist { get; } = herbalist;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Player.IsImpostorAligned() && !FreeForAllActive)
        {
            Player.RemoveModifier(this);
            return;
        }
        Visible = FreeForAllActive ? Herbalist.AmOwner : PlayerControl.LocalPlayer.IsImpostorAligned();
    }
}