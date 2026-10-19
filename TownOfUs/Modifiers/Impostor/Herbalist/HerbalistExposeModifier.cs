using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;

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

    public override void OnActivate()
    {
        base.OnActivate();
        MiraEventManager.InvokeEvent(new TouAbilityEvent(AbilityType.HerbalistExpose, Herbalist, Player));
    }

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