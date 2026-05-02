using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

public sealed class ArsonistDousedModifier(byte arsonistId) : BaseModifier
{
    public override string ModifierName => "Doused";
    public override bool HideOnUi => true;
    public byte ArsonistId { get; } = arsonistId;

    public override void OnActivate()
    {
        var arso = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == ArsonistId);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.ArsonistDouse, arso!, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
        ArsonistRole.SetDouseUses();
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
        {
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(Color.yellow));
        }
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(Color.yellow));
    }
}