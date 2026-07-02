using MiraAPI.Events;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Neutral;
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
        var arso = PlayerControl.AllPlayerControls.FirstOrDefault(x => x.PlayerId == ArsonistId);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.ArsonistDouse, arso!, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
        AmongUsClient.Instance.StartCoroutine(ArsonistDouseButton.CoSetDouses());
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
        {
            Player?.cosmetics.SetOutline(true, (Color.yellow));
        }
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, (Color.yellow));
    }
}