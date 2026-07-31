using MiraAPI.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class WarlockMarkedModifier : BaseModifier
{
    public override string ModifierName => "Marked";
    public override bool HideOnUi => true;
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Player)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }
        Player.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Impostor));
    }
    public override void OnDeath(DeathReason reason)
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Impostor));
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Impostor));
    }
}