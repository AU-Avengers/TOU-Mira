using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class SpellslingerHexedModifier : BaseModifier
{
    private readonly Color color = TownOfUsColors.ImpSoft;
    public override string ModifierName => "Hexed";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        var pb = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(x => x.IsRole<SpellslingerRole>());
        if (pb != null)
        {
            var touAbilityEvent = new TouAbilityEvent(AbilityType.SpellslingerHex, pb, Player);
            MiraEventManager.InvokeEvent(touAbilityEvent);
        }
    }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole<SpellslingerRole>() && Player != PlayerControl.LocalPlayer)
        {
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(color));
        }
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(color));
    }
}
