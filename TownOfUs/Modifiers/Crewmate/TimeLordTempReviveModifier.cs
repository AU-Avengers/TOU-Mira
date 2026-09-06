using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class TimeLordTempReviveModifier(PlayerControl timeLord) : BaseModifier
{
    public override string ModifierName => "Temporarily Revived";
    public override bool HideOnUi => true;
    public PlayerControl TimeLord = timeLord;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}